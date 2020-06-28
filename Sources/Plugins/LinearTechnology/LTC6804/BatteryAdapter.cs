using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using ImpruvIT.Contracts;
using ImpruvIT.Diagnostics;
using ImpruvIT.Threading;
using log4net;

using ImpruvIT.BatteryMonitor.Domain.Battery;
using ImpruvIT.BatteryMonitor.Domain.Descriptors;
using ImpruvIT.BatteryMonitor.Hardware;

namespace ImpruvIT.BatteryMonitor.Protocols.LinearTechnology.LTC6804
{
	public class BatteryAdapter : IBatteryAdapter
	{
		private const float CtoKCoefficient = 273.15f;
		private const float MinConnectedCellVoltage = 0.5f;

		private readonly object m_lock = new object();
		private readonly RepeatableTask m_monitoringTask;

		public BatteryAdapter(ICommunicateToBus busConnection)
		{
			Contract.Requires(busConnection, "busConnection").NotToBeNull();

			this.Tracer = LogManager.GetLogger(this.GetType());
			this.BusConnection = busConnection;
			this.m_monitoringTask = new RepeatableTask(this.MonitoringAction, "LTC6804 Monitor")
			{
				MinTriggerTime =  TimeSpan.FromSeconds(1)
			};
		}

		protected ILog Tracer { get; }
		public ICommunicateToBus BusConnection { get; set; }
		private LTC6804_1Interface Connection { get; set; }

		public int ChainLength => Connection?.ChainLength ?? 0;

		public Pack Pack
		{
			get { return this.m_pack; }
			private set
			{
				if (ReferenceEquals(this.m_pack, value))
					return;

				this.m_pack = value;
				this.OnPropertyChanged("Pack");
			}
		}
		private Pack m_pack;


		#region Battery recognition

		public async Task RecognizeBattery()
		{
			this.Tracer.DebugFormat("Recognizing battery ...");

			try
			{
                //await new LTC6804_1Interface(this.BusConnection, 1).WakeUp();
                //Thread.Sleep(10);
                await DetermineChainLength().ConfigureAwait(false);     // Wakeup

                // Determine chain length
                var chainLength = await DetermineChainLength().ConfigureAwait(false);
				if (chainLength == 0)
				{
					this.Tracer.Warn("No chips detected in the daisy chain.");

					this.Connection = null;
					this.Pack = null;
					return;
				}
				this.Connection = new LTC6804_1Interface(this.BusConnection, chainLength);

				// Setup chain
				await SetupChain().ConfigureAwait(false);

				// Determine geometry
				var pack = await this.DetermineGeometry().ConfigureAwait(false);

				this.Pack = pack;
				this.Tracer.InfoFormat("Battery recognized: {0} {1} ({2:F2} V, {3:N0} mAh).", pack.ProductDefinition().Manufacturer, pack.ProductDefinition().Product, pack.DesignParameters().NominalVoltage, pack.DesignParameters().DesignedCapacity * 1000);
			}
			catch (Exception ex)
			{
				Exception thrownEx = ex;
				if (thrownEx is AggregateException aggregateEx)
					thrownEx = aggregateEx.Flatten().InnerException;

				this.Tracer.Warn("Error while reading health information of the battery", thrownEx);

				if (!(thrownEx is InvalidOperationException))
					throw;
			}

			this.OnDescriptorsChanged();
		}

		private async Task<int> DetermineChainLength()
		{
			// Read status register with chain length of 32
			var fullChainConnection = new LTC6804_1Interface(this.BusConnection, 32);
			var statusBChainData = await fullChainConnection.ReadRegister(CommandId.ReadStatusRegisterB, 6).ConfigureAwait(false);

			// Check how many answers we got
			var chainLength = statusBChainData.Select(d => d != null && d.Any(b => b != 0xFF)).Select((x, i) => x ? (i + 1) : 0).Max();

			return chainLength;
		}

		private async Task SetupChain()
		{
			// Setup default configuration register
			var configRegister = new ConfigurationRegister();
			configRegister.SetGpioPullDowns(false);
			configRegister.ReferenceOn = false;
			configRegister.UnderVoltage = 2.7f;
			configRegister.OverVoltage = 4.2f;
			for (int i = 0; i < 12; i++)
				configRegister.SetDischargeSwitch(i, false);
			configRegister.SetDischargeTimeout(DischargeTime.Disabled);

			await this.Connection.WriteRegister(CommandId.WriteConfigRegister, configRegister.Data).ConfigureAwait(false);
		}

		private async Task<Pack> DetermineGeometry()
		{
			// Read all cell voltages in the whole chain
			await this.Connection.ExecuteCommand(CommandId.StartCellConversion(ConversionMode.Normal, false, 0)).ConfigureAwait(false);
			await Task.Delay(3).ConfigureAwait(false);

			// Read voltages
			var cellVoltageA = (await this.Connection.ReadRegister(CommandId.ReadCellRegisterA, 6).ConfigureAwait(false)).ToArray();
			var cellVoltageB = (await this.Connection.ReadRegister(CommandId.ReadCellRegisterB, 6).ConfigureAwait(false)).ToArray();
			var cellVoltageC = (await this.Connection.ReadRegister(CommandId.ReadCellRegisterC, 6).ConfigureAwait(false)).ToArray();
			var cellVoltageD = (await this.Connection.ReadRegister(CommandId.ReadCellRegisterD, 6).ConfigureAwait(false)).ToArray();
			this.CheckChainLength("determining pack geometry", cellVoltageA, cellVoltageB, cellVoltageC, cellVoltageD);

			// Build a series pack for each IC in chain
			var chainPacks = new List<ChipPack>();
			for (int chainIndex = 0; chainIndex < this.ChainLength; chainIndex++)
			{
				// Decode cell voltages
				var voltageRegister = CellVoltageRegister.FromGroups(cellVoltageA[chainIndex], cellVoltageB[chainIndex], cellVoltageC[chainIndex], cellVoltageD[chainIndex]);
				var cellVoltages = Enumerable.Range(1, 12)
					.Select(x => Tuple.Create(x, voltageRegister.GetCellVoltage(x)));

				// Detect connected cells
				var packCells = cellVoltages
					.Where(x => x.Item2 > MinConnectedCellVoltage)
					.ToDictionary(x => x.Item1,
						x =>
						{
							var cell = new SingleCell();
							cell.Actuals().Voltage = x.Item2;
							return cell;
						});

				var chipPack = new ChipPack(chainIndex, packCells);

				chainPacks.Add(chipPack);
			}

			this.Tracer.Debug(new TraceBuilder()
					.AppendLine("A LTC6804 daisy chain with {0} chips recognized with following geometry:", this.ChainLength)
					.Indent()
						.ForEach(chainPacks, (tb, p) => tb
							.AppendLine("Chain index {0} with {1} connected cells:", p.ChainIndex, p.ConnectedCells.Count)
							.Indent()
								.AppendLineForEach(p.ConnectedCells, "Channel {0} => {1} V", x => x.Key, x => x.Value.Actuals().Voltage)
							.Unindent()
							.AppendLine())
					.Trace());

			// Connect all chip packs into a series stack
            Pack pack = chainPacks.Count > 1 ? new SeriesPack(chainPacks) : chainPacks[0];

			return pack;
		}

		#endregion Battery recognition


		#region Readings

		public Task UpdateReadings()
		{
			return this.ReadActuals();
		}

		private async Task ReadActuals()
		{
			this.Tracer.DebugFormat("Reading battery actuals information of the battery.");

			var batteryPack = this.Pack;
			if (batteryPack == null)
				return;

			if (this.ChainLength == 0)
			{
				this.Tracer.Warn("Unable to read actuals as no chips were detected in the daisy chain.");
				return;
			}

			if (batteryPack is ChipPack && this.ChainLength > 1)
			{
				this.Tracer.Warn($"Unable to read actuals as battery geometry has single chip while daisy chain has {this.ChainLength} chips.");
				return;
			}

			try
			{
				// Wake up
				await this.Connection.ReadRegister(CommandId.ReadConfigRegister, 6).ConfigureAwait(false);

				// Measure
				await this.MeasureActuals();

				// Read data
				var cellVoltageRegisters = await this.ReadCellVoltages();
				var auxVoltageRegisters = await this.ReadAuxVoltages();
				var statusRegisters = await this.ReadStatusRegister();
				
				// Process data
				for (int chainIndex = 0; chainIndex < this.ChainLength; chainIndex++)
				{
					// Find chip pack
					var chipPack = this.FindChipPack(chainIndex);
					if (chipPack == null)
					{
						this.Tracer.Warn($"A chip pack with chain index {chainIndex} was not found in the stack while processing actuals. Ignoring measured data for this chip.");
						continue;
					}

					var chipPackActuals = chipPack.Actuals();

					// Decode measured data
					var cellsRegister = cellVoltageRegisters[chainIndex];
					var auxRegister = auxVoltageRegisters[chainIndex];
					var statusRegister = statusRegisters[chainIndex];

					chipPackActuals.Voltage = statusRegister.PackVoltage;
					chipPackActuals.Temperature = ConvertToTemperature(auxRegister.GetAuxVoltage(3), auxRegister.Ref2Voltage);
					
					// Update actuals for each connected cell
					foreach (var connectedCell in chipPack.ConnectedCells)
					{
						var cellActuals = connectedCell.Value.Actuals();

						var cellVoltage = cellsRegister.GetCellVoltage(connectedCell.Key);
						cellActuals.Voltage = cellVoltage;
					}

					this.Tracer.Debug(new TraceBuilder()
						.AppendLine("The actuals of the chip pack with chain index {0} successfully read:", chainIndex)
						.Indent()
							.AppendLine("Pack voltage:            {0} V", chipPackActuals.Voltage)
							.AppendLineForEach(chipPack.ConnectedCells, "Cell {0:2} voltage:      {1} V", x => x.Key, x => x.Value.Actuals().Voltage)
						.Trace());
				}
			}
			catch (Exception ex)
			{
				Exception thrownEx = ex;
				if (thrownEx is AggregateException aggregateEx)
					thrownEx = aggregateEx.Flatten().InnerException;

				this.Tracer.Warn("Error while reading actuals of the battery", thrownEx);

				if (!(thrownEx is InvalidOperationException))
					throw;
			}
		}

		private async Task MeasureActuals()
		{
			// Start reference
			await StartReference().ConfigureAwait(false);

			// Measure everything
			await this.Connection.ExecuteCommand(CommandId.StartCellConversion(ConversionMode.Normal, false, 0)).ConfigureAwait(false);
			await Task.Delay(3).ConfigureAwait(false);
			await this.Connection.ExecuteCommand(CommandId.StartAuxConversion(ConversionMode.Normal, 0)).ConfigureAwait(false);
			await Task.Delay(3).ConfigureAwait(false);
			await this.Connection.ExecuteCommand(CommandId.StartStatusConversion(ConversionMode.Normal, 1)).ConfigureAwait(false);
			await Task.Delay(2).ConfigureAwait(false);

			// Shutdown reference
			await StopReference().ConfigureAwait(false);
		}

		#endregion Readings


		#region Monitoring

		private readonly List<UpdatesSubscription> m_subscriptions = new List<UpdatesSubscription>();

		public ISubscription SubscribeToUpdates(Action<Pack> notificationConsumer, UpdateFrequency frequency = UpdateFrequency.Normal)
		{
			lock (this.m_lock)
			{
				var subscription = new UpdatesSubscription(notificationConsumer, frequency, this.UnsubscribeConsumer);
				this.m_subscriptions.Add(subscription);
				this.UpdateMonitoringTask();

				return subscription;
			}
		}

		private void UnsubscribeConsumer(UpdatesSubscription subscription)
		{
			lock (this.m_lock)
			{
				this.m_subscriptions.Remove(subscription);
				this.UpdateMonitoringTask();
			}
		}

		private void UpdateMonitoringTask()
		{
			// Method has to be called within lock

			var hasSubscribers = this.m_subscriptions.Any();
			if (hasSubscribers && !this.m_monitoringTask.IsRunning)
			{
				this.m_monitoringTask.Start();
				this.Tracer.InfoFormat("Monitoring of the battery started.");
			}
			else if (!hasSubscribers && this.m_monitoringTask.IsRunning)
			{
				this.m_monitoringTask.Stop();
				this.Tracer.InfoFormat("Monitoring of the battery stopped.");
			}
				

			// TODO: update period
		}

		private void MonitoringAction()
		{
			// Read values
			this.ReadActuals().Wait();

			List<Task> tasks;
			lock (this.m_lock)
			{
				tasks = this.m_subscriptions.Select(x => x.Consumer).Select(x => Task.Factory.StartNew(() => x(this.Pack))).ToList();
			}

			Task.WhenAll(tasks).ContinueWith(this.ConsumerErrorHandler, TaskContinuationOptions.OnlyOnFaulted);
		}

		private void ConsumerErrorHandler(Task task)
		{
			var ex = task.Exception;
			this.Tracer.Warn("Update notification consumer failed.", ex);

			// Exception intentionally swallowed
		}

		#endregion Monitoring


		#region Self-test

		private const int OpenWireMeasurementCount = 5;
		private const ushort DigitalFilterTestAdcValue = 0x9555;
		private const float MaxPackVoltageDiff = 0.5f;
		private const float MinAnalogSupplyVoltage = 4.5f;
		private const float MaxAnalogSupplyVoltage = 5.5f;
		private const float MinDigitalSupplyVoltage = 2.7f;
		private const float MaxDigitalSupplyVoltage = 3.6f;
		private const float MinRef2Voltage = 2.985f;
		private const float MaxRef2Voltage = 3.015f;
		private const float MaxDieTemperature = 80.0f + CtoKCoefficient;

		public async Task PerformSelfTest()
		{
			await this.StartReference().ConfigureAwait(false);
			await Task.Delay(4).ConfigureAwait(false);

			await this.TestDigitalFilter().ConfigureAwait(false);
			await this.TestVoltages().ConfigureAwait(false);
			//await this.TestOpenWires().ConfigureAwait(false);

			await this.StopReference().ConfigureAwait(false);
		}

		private async Task TestDigitalFilter()
		{
			await this.Connection.ExecuteCommand(CommandId.StartCellSelfTest(ConversionMode.Normal, SelfTestMode.Mode1)).ConfigureAwait(false);
			await Task.Delay(3).ConfigureAwait(false);
			await this.Connection.ExecuteCommand(CommandId.StartAuxSelfTest(ConversionMode.Normal, SelfTestMode.Mode1)).ConfigureAwait(false);
			await Task.Delay(3).ConfigureAwait(false);
			await this.Connection.ExecuteCommand(CommandId.StartStatusSelfTest(ConversionMode.Normal, SelfTestMode.Mode1)).ConfigureAwait(false);
			await Task.Delay(2).ConfigureAwait(false);

			var cellRegisters = await this.ReadCellVoltages().ConfigureAwait(false);
			var auxRegisters = await this.ReadAuxVoltages().ConfigureAwait(false);
			var statusRegisters = await this.ReadStatusRegister().ConfigureAwait(false);

			for (int chainIndex = 0; chainIndex < this.ChainLength; chainIndex++)
			{
				var buffer = new byte[24 + 12 + 4];
				Array.Copy(cellRegisters[chainIndex].Data, 0, buffer, 0, 24);
				Array.Copy(auxRegisters[chainIndex].Data, 0, buffer, 24, 12);
				Array.Copy(statusRegisters[chainIndex].Data, 0, buffer, 24 + 12, 4);

				var testFailed = false;
				for (int i = 0; i < buffer.Length; i += 2)
				{
					var actualValue = (ushort)((buffer[i + 1] << 8) + buffer[i]);
					if (actualValue != DigitalFilterTestAdcValue)
					{
						testFailed = true;
						break;
					}
				}

				if (testFailed)
					this.Tracer.Warn($"The digital filter of the chip pack with chain index {chainIndex} is broken.");
				else
					this.Tracer.Debug($"The digital filter of the chip pack with chain index {chainIndex} is OK.");
			}
		}

		private async Task TestVoltages()
		{
			await this.Connection.ExecuteCommand(CommandId.DiagnoseMux).ConfigureAwait(false);
			await Task.Delay(1).ConfigureAwait(false);
			await this.Connection.ExecuteCommand(CommandId.StartAuxConversion(ConversionMode.Normal, 6)).ConfigureAwait(false);
			await Task.Delay(1).ConfigureAwait(false);
			await this.Connection.ExecuteCommand(CommandId.StartStatusConversion(ConversionMode.Normal, 0)).ConfigureAwait(false);
			await Task.Delay(2).ConfigureAwait(false);

			var auxRegisters = await this.ReadAuxVoltages().ConfigureAwait(false);
			var statusRegisters = await this.ReadStatusRegister().ConfigureAwait(false);

			for (int chainIndex = 0; chainIndex < this.ChainLength; chainIndex++)
			{
				// Find chip pack
				var chipPack = this.FindChipPack(chainIndex);
				if (chipPack == null)
				{
					this.Tracer.Warn($"The chip pack with chain index {chainIndex} was not found in the stack while processing actuals. Ignoring measured data for this chip.");
					continue;
				}

				var auxRegister = auxRegisters[chainIndex];
				var statusRegister = statusRegisters[chainIndex];

				// Check pack voltage
				var packActuals = chipPack.Actuals();
				var packVoltageDiff = Math.Abs(packActuals.Voltage - statusRegister.PackVoltage);
				if (packVoltageDiff > MaxPackVoltageDiff)
					this.Tracer.Warn($"The chip pack with chain index {chainIndex} has too big pack voltage difference. Pack voltage: {statusRegister.PackVoltage:N2} V. Sum of cell voltages: {packActuals.Voltage:N2} V.");
				else
					this.Tracer.Debug($"The chip pack with chain index {chainIndex} has normal pack voltage difference. Pack voltage: {statusRegister.PackVoltage:N2} V. Sum of cell voltages: {packActuals.Voltage:N2} V.");

				// Check analog power supply voltage
				if (statusRegister.AnalogSupplyVoltage < MinAnalogSupplyVoltage || statusRegister.AnalogSupplyVoltage > MaxAnalogSupplyVoltage)
					this.Tracer.Warn($"The analog supply voltage of the chip pack with chain index {chainIndex} is out of range. Actual voltage {statusRegister.AnalogSupplyVoltage:N3} V. Expected range: {MinAnalogSupplyVoltage:N1} - {MaxAnalogSupplyVoltage:N1} V.");
				else
					this.Tracer.Debug($"The analog supply voltage of the chip pack with chain index {chainIndex} is in normal range. Actual voltage {statusRegister.AnalogSupplyVoltage:N3} V. Expected range: {MinAnalogSupplyVoltage:N1} - {MaxAnalogSupplyVoltage:N1} V.");

				// Check analog power supply voltage
				if (statusRegister.DigitalSupplyVoltage < MinDigitalSupplyVoltage || statusRegister.DigitalSupplyVoltage > MaxDigitalSupplyVoltage)
					this.Tracer.Warn($"The digital supply voltage of the chip pack with chain index {chainIndex} is out of range. Actual voltage {statusRegister.DigitalSupplyVoltage:N3} V. Expected range: {MinDigitalSupplyVoltage:N1} - {MaxDigitalSupplyVoltage:N1} V.");
				else
					this.Tracer.Debug($"The digital supply voltage of the chip pack with chain index {chainIndex} is in normal range. Actual voltage {statusRegister.DigitalSupplyVoltage:N3} V. Expected range: {MinDigitalSupplyVoltage:N1} - {MaxDigitalSupplyVoltage:N1} V.");

				// Check 2nd reference voltage
				if (auxRegister.Ref2Voltage < MinRef2Voltage || auxRegister.Ref2Voltage > MaxRef2Voltage)
					this.Tracer.Warn($"The 2nd reference voltage of the chip pack with chain index {chainIndex} is out of range. Actual voltage {auxRegister.Ref2Voltage:N3} V. Expected range: {MinRef2Voltage:N3} - {MaxRef2Voltage:N3} V.");
				else
					this.Tracer.Debug($"The 2nd reference voltage of the chip pack with chain index {chainIndex} is normal range. Actual voltage {auxRegister.Ref2Voltage:N3} V. Expected range: {MinRef2Voltage:N3} - {MaxRef2Voltage:N3} V.");

				// Check failing mux
				if (statusRegister.MuxFail)
					this.Tracer.Warn($"The mux of the chip pack with chain index {chainIndex} has failure.");
				else
					this.Tracer.Debug($"The mux of the chip pack with chain index {chainIndex} is OK.");

				// Check die temperature
				if (statusRegister.DieTemperature > MaxDieTemperature)
					this.Tracer.Warn($"The die temperature of the chip pack with chain index {chainIndex} is too high. Actual temperature {statusRegister.DieTemperature - CtoKCoefficient:N1} °C. Maximum allowed temperature: {MaxDieTemperature - CtoKCoefficient:N1} °C.");
				else
					this.Tracer.Debug($"The die temperature of the chip pack with chain index {chainIndex} is in operational range. Actual temperature {statusRegister.DieTemperature - CtoKCoefficient:N1} °C. Maximum allowed temperature: {MaxDieTemperature - CtoKCoefficient:N1} °C.");

				// Check whether chip shut down because of high temperature
				if (statusRegister.ThermalShutdownOccurred)
					this.Tracer.Warn($"The chip pack with chain index {chainIndex} registered a thermal shut down.");
				else
					this.Tracer.Debug($"The chip pack with chain index {chainIndex} did not register any thermal shut down.");
			}
		}

		private async Task TestOpenWires()
		{
			// Measure cell voltages with current pull-up
			for (int i = 0; i < OpenWireMeasurementCount; i++)
			{
				await this.Connection.ExecuteCommand(CommandId.StartOpenWireConversion(ConversionMode.Normal, true, false, 0)).ConfigureAwait(false);
				await Task.Delay(10).ConfigureAwait(false);
			}
			
			// Read cell voltages
			var cellRegisters = await this.ReadCellVoltages().ConfigureAwait(false);
			var pullUpVoltages = cellRegisters
				.Select(x =>
					Enumerable.Range(1, 12)
						.Select(x.GetCellVoltage)
						.ToArray())
				.ToArray();

			// Measure cell voltages with current pull-down
			for (int i = 0; i < OpenWireMeasurementCount; i++)
			{
				await this.Connection.ExecuteCommand(CommandId.StartOpenWireConversion(ConversionMode.Normal, false, false, 0)).ConfigureAwait(false);
				await Task.Delay(10).ConfigureAwait(false);
			}

			// Read cell voltages
			cellRegisters = await this.ReadCellVoltages().ConfigureAwait(false);
			var pullDownVoltages = cellRegisters
				.Select(x =>
					Enumerable.Range(1, 12)
						.Select(x.GetCellVoltage)
						.ToArray())
				.ToArray();

			for (int chainIndex = 0; chainIndex < this.ChainLength; chainIndex++)
			{
				var openWires = new List<int>();
				if (Math.Abs(pullUpVoltages[chainIndex][0] - 0.0f) < Single.Epsilon)
					openWires.Add(0);
				for (int i = 0; i <= 10; i++)
				{
					var cellVoltageDiff = pullUpVoltages[chainIndex][i + 1] - pullDownVoltages[chainIndex][i + 1];
					if (cellVoltageDiff < -0.4f)
						openWires.Add(i + 1);
				}
				if (Math.Abs(pullDownVoltages[chainIndex][11] - 0.0f) < Single.Epsilon)
					openWires.Add(12);

                if (openWires.Count > 0)
                {
                    var openWiresMsg = openWires.Select(x => x.ToString(CultureInfo.CurrentCulture)).Join(", ");
                    this.Tracer.Warn($"There are open wires on the chip pack with chain index {chainIndex}. Open wires: {openWiresMsg}.");
                }
				else
					this.Tracer.Debug($"All cell voltage wires on the chip pack with chain index {chainIndex} are correctly connected.");
			}
		}

		#endregion Self-test


		#region Primitives

		private async Task StartReference()
		{
			byte[][] configRegisterData;
			do {
				configRegisterData = (await this.Connection.ReadRegister(CommandId.ReadConfigRegister, 6).ConfigureAwait(false)).ToArray();
				this.CheckChainLength("reading configuration register", configRegisterData);
			} while(configRegisterData.Any(x => x == null));

			var configRegisters = configRegisterData.Select(x => new ConfigurationRegister(x)).ToArray();

			configRegisters.ForEach(x => x.ReferenceOn = true);
			await this.Connection.WriteRegister(CommandId.WriteConfigRegister, configRegisters.Select(x => x.Data)).ConfigureAwait(false);
		}

		private async Task StopReference()
		{
			byte[][] configRegisterData;
			do
			{
				configRegisterData = (await this.Connection.ReadRegister(CommandId.ReadConfigRegister, 6).ConfigureAwait(false)).ToArray();
				this.CheckChainLength("reading configuration register", configRegisterData);
			} while (configRegisterData.Any(x => x == null));

			var configRegisters = configRegisterData.Select(x => new ConfigurationRegister(x)).ToArray();

			configRegisters.ForEach(x => x.ReferenceOn = false);
			await this.Connection.WriteRegister(CommandId.WriteConfigRegister, configRegisters.Select(x => x.Data)).ConfigureAwait(false);
		}

		private async Task<CellVoltageRegister[]> ReadCellVoltages()
		{
			var cellVoltageA = (await this.Connection.ReadRegister(CommandId.ReadCellRegisterA, 6).ConfigureAwait(false)).ToArray();
			var cellVoltageB = (await this.Connection.ReadRegister(CommandId.ReadCellRegisterB, 6).ConfigureAwait(false)).ToArray();
			var cellVoltageC = (await this.Connection.ReadRegister(CommandId.ReadCellRegisterC, 6).ConfigureAwait(false)).ToArray();
			var cellVoltageD = (await this.Connection.ReadRegister(CommandId.ReadCellRegisterD, 6).ConfigureAwait(false)).ToArray();
			this.CheckChainLength("reading cell voltages", cellVoltageA, cellVoltageB, cellVoltageC, cellVoltageD);

			var voltageRegisters = Enumerable.Range(0, this.ChainLength)
				.Select(x => CellVoltageRegister.FromGroups(cellVoltageA[x], cellVoltageB[x], cellVoltageC[x], cellVoltageD[x]))
				.ToArray();

			return voltageRegisters;
		}

		private async Task<AuxVoltageRegister[]> ReadAuxVoltages()
		{
			var auxA = (await this.Connection.ReadRegister(CommandId.ReadAuxRegisterA, 6).ConfigureAwait(false)).ToArray();
			var auxB = (await this.Connection.ReadRegister(CommandId.ReadAuxRegisterB, 6).ConfigureAwait(false)).ToArray();
			this.CheckChainLength("reading auxiliary voltages", auxA, auxB);

			var auxRegisters = Enumerable.Range(0, this.ChainLength)
				.Select(x => AuxVoltageRegister.FromGroups(auxA[x], auxB[x]))
				.ToArray();

			return auxRegisters;
		}

		private async Task<StatusRegister[]> ReadStatusRegister()
		{
			var statusRegisterA = (await this.Connection.ReadRegister(CommandId.ReadStatusRegisterA, 6).ConfigureAwait(false)).ToArray();
			var statusRegisterB = (await this.Connection.ReadRegister(CommandId.ReadStatusRegisterB, 6).ConfigureAwait(false)).ToArray();
			this.CheckChainLength("reading status registers", statusRegisterA, statusRegisterB);

			var statusRegisters = Enumerable.Range(0, this.ChainLength)
				.Select(x => StatusRegister.FromGroups(statusRegisterA[x], statusRegisterB[x]))
				.ToArray();

			return statusRegisters;
		}

		#endregion Primitives


		#region Descriptions

		public IEnumerable<ReadingDescriptorGrouping> GetDescriptors()
		{
			var pack = this.Pack;
			if (pack == null)
				yield break;

			IEnumerable<ReadingDescriptor> cellVoltageDescriptors;
			if (pack is ChipPack)
			{
				cellVoltageDescriptors = this.Pack.SubElements
					.Select((x, i) => LtReadingDescriptors.CreateSingleChipCellVoltageDescriptor(i));
			}
			else
			{
				cellVoltageDescriptors = this.Pack.SubElements
					.OfType<ChipPack>()
					.OrderBy(x => x.ChainIndex)
					.SelectMany(x => x.SubElements
						.Select((c, i) => Tuple.Create(x.ChainIndex, i)))
					.Select(t => LtReadingDescriptors.CreateChainCellVoltageDescriptor(t.Item1, t.Item2));
			}

			var actualDescriptors = new List<ReadingDescriptor>();
			actualDescriptors.Add(ReadingDescriptors.PackVoltage);
			actualDescriptors.Add(LtReadingDescriptors.SumOfCellVoltages);
			actualDescriptors.AddRange(cellVoltageDescriptors);
			actualDescriptors.Add(ReadingDescriptors.Temperature);
			
			yield return new ReadingDescriptorGrouping(
				"Actuals",
				actualDescriptors)
			{
				IsDefault = true
			};
		}

		/// <inheritdoc />
		public event EventHandler DescriptorsChanged;

		/// <summary>
		/// Fires the <see cref="DescriptorsChanged"/> event.
		/// </summary>
		protected virtual void OnDescriptorsChanged()
		{
            this.DescriptorsChanged?.Invoke(this, EventArgs.Empty);
        }

		#endregion Descriptions


		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Fires the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">The name of the changed property.</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

		private void CheckChainLength(string actionDescription, params byte[][][] chainData)
		{
			if (chainData.Any(x => x.Length != this.ChainLength))
            {
                var detectedLengths = chainData.Select(x => x.Length.ToString(CultureInfo.InvariantCulture)).Join(", ");
				var message = $"Inconsistent daisy chain length detected while {actionDescription}. Lengths detected: {detectedLengths}";

				this.Tracer.Error(message);
				throw new InvalidOperationException(message);
			}
		}

		private static float ConvertToTemperature(float voltage, float ref2Voltage)
		{
			const float coefficientA = 0.003825269f;
			const float coefficientB = -27.64f;

			var adcReading = voltage / ref2Voltage * 30000;
			var temperature = coefficientA * adcReading + coefficientB;
			return temperature + CtoKCoefficient;
		}

		private ChipPack FindChipPack(int chainIndex)
		{
			var pack = this.Pack;

			var chipPack = pack as ChipPack;
			if (chipPack != null && chipPack.ChainIndex != chainIndex)
				chipPack = null;

			if (chipPack == null)
			{
				chipPack = pack.SubElements
					.OfType<ChipPack>()
					.FirstOrDefault(x => x.ChainIndex == chainIndex);
			}

			return chipPack;
		}
    }
}
