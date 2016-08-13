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

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Domain.Description;
using ImpruvIT.BatteryMonitor.Hardware;

namespace ImpruvIT.BatteryMonitor.Protocols.LinearTechnology.LTC6804
{
	public class BatteryAdapter : IBatteryPackAdapter
	{
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

		protected ILog Tracer { get; private set; }
		public ICommunicateToBus BusConnection { get; set; }
		protected LTC6804_1Interface Connection { get; private set; }

		public int ChainLength 
		{
			get { return this.Connection.ChainLength; }
		}

		public BatteryPack Pack
		{
			get { return this.m_pack; }
			private set
			{
				if (Object.ReferenceEquals(this.m_pack, value))
					return;

				this.m_pack = value;
				this.OnPropertyChanged("Pack");
			}
		}
		private BatteryPack m_pack;


		#region Battery recognition

		public async Task RecognizeBattery()
		{
			this.Tracer.DebugFormat("Recognizing battery ...");

			try
			{
				// Determine chain length
				var chainLength = await DetermineChainLength().ConfigureAwait(false);
				this.Connection = new LTC6804_1Interface(this.BusConnection, chainLength);

				// Setup chain
				await SetupChain().ConfigureAwait(false);

				// Determine geometry
				var pack = await this.DetermineGeometry().ConfigureAwait(false);
				await this.ReadProductData(pack).ConfigureAwait(false);

				this.Pack = pack;
				this.Tracer.InfoFormat("Battery recognized: {0} {1} ({2:F2} V, {3:N0} mAh).", pack.Product.Manufacturer, pack.Product.Product, pack.DesignParameters.NominalVoltage, pack.DesignParameters.DesignedCapacity * 1000);
			}
			catch (Exception ex)
			{
				Exception thrownEx = ex;
				if (thrownEx is AggregateException)
					thrownEx = ((AggregateException)thrownEx).Flatten().InnerException;

				this.Tracer.Warn(String.Format("Error while reading health information of the battery"), thrownEx);

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
			var chainLength = statusBChainData.Select(d => d != null && d.Any(b => b != 0xFF)).Select((x, i) => x ? i : 0).Max() + 1;

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

		private async Task<BatteryPack> DetermineGeometry()
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

			const float cellVoltage = 3.6f;
			const float designedDischargeCurrent = 1.0f;
			const float maxDischargeCurrent = 2.0f;
			const float designedCapacity = 1.1f;

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
							var cell = new SingleCell(cellVoltage, designedDischargeCurrent, maxDischargeCurrent, designedCapacity);
							cell.SetVoltage(x.Item2);
							return cell;
						});

				var pack = new ChipPack(chainIndex, packCells);
				chainPacks.Add(pack);
			}

			// Connect all chip packs into a series stack
			BatteryPack batteryPack = null;
			//if (packs.Count > 1)
				batteryPack = new SeriesBatteryPack(chainPacks);
			//else
			//	batteryPack = packs[0];

			this.Tracer.Debug(new TraceBuilder()
					.AppendLine("A LTC6804 daisy chain with {0} chips recognized with following geometry:", this.ChainLength)
					.Indent()
						.ForEach(chainPacks, (tb, p) => tb
							.AppendLine("Chain index {0} with {1} connected cells:", p.ChainIndex, p.ConnectedCells.Count)
							.Indent()
								.AppendLineForEach(p.ConnectedCells, "Channel {0} => {1} V", x => x.Key, x => x.Value.Actuals.Voltage)
							.Unindent()
							.AppendLine())
					.Trace());

			return batteryPack;
		}

		private Task ReadProductData(BatteryPack pack)
		{
			this.Tracer.DebugFormat("Reading manufacturer data of battery ...");

			var productDefinitionWrapper = new ProductDefinitionWrapper(pack.CustomData);
			productDefinitionWrapper.Manufacturer = "Linear Technology";
			productDefinitionWrapper.Product = "LTC6804";
			productDefinitionWrapper.Chemistry = "LiFePO4";
			productDefinitionWrapper.ManufactureDate = new DateTime(2016, 1, 1);
			productDefinitionWrapper.SerialNumber = "SN123456789";

			this.Tracer.Debug(new TraceBuilder()
					.AppendLine("The manufacturer data of battery:")
					.Indent()
						.AppendLine("Manufacturer:     {0}", pack.Product.Manufacturer)
						.AppendLine("Product:          {0}", pack.Product.Product)
						.AppendLine("Chemistry:        {0}", pack.Product.Chemistry)
						.AppendLine("Manufacture date: {0}", pack.Product.ManufactureDate.ToShortDateString())
						.AppendLine("Serial number:    {0}", pack.Product.SerialNumber)
					.Trace());

			return Task.CompletedTask;
		}

		#endregion Battery recognition


		#region Readings

		public Task ReadHealth()
		{
			var pack = this.Pack;
			if (pack == null)
				return Task.CompletedTask;

			this.Tracer.DebugFormat("Reading battery health information of the battery.");

			return Task.CompletedTask;
		}

		public async Task ReadActuals()
		{
			this.Tracer.DebugFormat("Reading battery actuals information of the battery.");

			var batteryPack = this.Pack;
			if (batteryPack == null)
				return;

			try
			{
				// Start reference
				var configRegisterChainData = await this.Connection.ReadRegister(CommandId.ReadConfigRegister, 6).ConfigureAwait(false);
				var configRegisters = configRegisterChainData.Select(x => new ConfigurationRegister(x)).ToArray();
				configRegisters.ForEach(x => x.SetGpioPullDowns(false));
				configRegisters.ForEach(x => x.ReferenceOn = true);
				await this.Connection.WriteRegister(CommandId.WriteConfigRegister, configRegisters.Select(x => x.Data)).ConfigureAwait(false);

				// Measure everything
				await this.Connection.ExecuteCommand(CommandId.StartCellConversion(ConversionMode.Normal, false, 0)).ConfigureAwait(false);
				await Task.Delay(5).ConfigureAwait(false);
				await this.Connection.ExecuteCommand(CommandId.StartAuxConversion(ConversionMode.Normal, 0)).ConfigureAwait(false);
				await Task.Delay(5).ConfigureAwait(false);
				//await this.Connection.ExecuteCommand(CommandId.StartStatusConversion(ConversionMode.Normal, 1)).ConfigureAwait(false);
				//await Task.Delay(2).ConfigureAwait(false);

				// Shutdown reference
				configRegisters.ForEach(x => x.ReferenceOn = false);
				await this.Connection.WriteRegister(CommandId.WriteConfigRegister, configRegisters.Select(x => x.Data)).ConfigureAwait(false);

				// Read data
				var cellVoltageA = (await this.Connection.ReadRegister(CommandId.ReadCellRegisterA, 6).ConfigureAwait(false)).ToArray();
				var cellVoltageB = (await this.Connection.ReadRegister(CommandId.ReadCellRegisterB, 6).ConfigureAwait(false)).ToArray();
				var cellVoltageC = (await this.Connection.ReadRegister(CommandId.ReadCellRegisterC, 6).ConfigureAwait(false)).ToArray();
				var cellVoltageD = (await this.Connection.ReadRegister(CommandId.ReadCellRegisterD, 6).ConfigureAwait(false)).ToArray();
				var auxA = (await this.Connection.ReadRegister(CommandId.ReadAuxRegisterA, 6).ConfigureAwait(false)).ToArray();
				var auxB = (await this.Connection.ReadRegister(CommandId.ReadAuxRegisterB, 6).ConfigureAwait(false)).ToArray();
				this.CheckChainLength("reading actuals", cellVoltageA, cellVoltageB, cellVoltageC, cellVoltageD, auxA, auxB);

				// Process data
				var actualCurrent = 0.0f;
				var averageCurrent = 0.0f;

				var remainingCapacity = 1.0f;
				var absoluteStateOfCharge = 1.0f;
				var relativeStateOfCharge = 1.0f;
				TimeSpan actualRunTime, averageRunTime;
				if (actualCurrent >= 0)
				{
					actualRunTime = TimeSpan.Zero;
					averageRunTime = TimeSpan.Zero;
				}
				else
				{
					actualRunTime = TimeSpan.Zero;
					averageRunTime = actualRunTime;
				}

				for (int chainIndex = 0; chainIndex < this.ChainLength; chainIndex++)
				{
					// Find chip pack
					var chipPack = batteryPack.SubElements
						.OfType<ChipPack>()
						.FirstOrDefault(x => x.ChainIndex == chainIndex);
					if (chipPack == null)
					{
						this.Tracer.Warn(String.Format("A chip pack with chain index {0} was not found in the stack while processing actuals. Ignoring measured data for this chip.", chainIndex));
						continue;
					}

					// Decode measured data
					var cellsRegister = CellVoltageRegister.FromGroups(cellVoltageA[chainIndex], cellVoltageB[chainIndex], cellVoltageC[chainIndex], cellVoltageD[chainIndex]);
					var auxRegister = AuxVoltageRegister.FromGroups(auxA[chainIndex], auxB[chainIndex]);

					//var packVoltage = (await this.ReadUShortValue(SMBusCommandIds.Voltage).ConfigureAwait(false)) / 1000f;
					//var ref2Voltage = auxRegister.Ref2Voltage;
					//var temperature = auxRegister.GetAuxVoltage(1);
					var temperature = 298.0f;
					
					// Update actuals for each connected cell
					foreach (var connectedCell in chipPack.ConnectedCells)
					{
						var cell = connectedCell.Value;
						//cell.BeginUpdate();
						//try
						//{

						var cellVoltage = cellsRegister.GetCellVoltage(connectedCell.Key);
						cell.SetVoltage(cellVoltage);
						cell.SetActualCurrent(actualCurrent);
						cell.SetAverageCurrent(averageCurrent);
						cell.SetTemperature(temperature);

						cell.SetRemainingCapacity(remainingCapacity);
						cell.SetAbsoluteStateOfCharge(absoluteStateOfCharge);
						cell.SetRelativeStateOfCharge(relativeStateOfCharge);
						cell.SetActualRunTime(actualRunTime);
						cell.SetAverageRunTime(averageRunTime);

						//}
						//finally
						//{
						//	conditions.EndUpdate();
						//}
					}

					var actuals = chipPack.Actuals;

					this.Tracer.Debug(new TraceBuilder()
						.AppendLine("The actuals of the chip pack with chain index {0} successfully read:", chainIndex)
						.Indent()
							.AppendLine("Pack voltage:            {0} V", actuals.Voltage)
							.AppendLineForEach(chipPack.ConnectedCells, "Cell {0:2} voltage:      {1} V", x => x.Key, x => x.Value.Actuals.Voltage)
						.Trace());
				}
			}
			catch (Exception ex)
			{
				Exception thrownEx = ex;
				if (thrownEx is AggregateException)
					thrownEx = ((AggregateException)thrownEx).Flatten().InnerException;

				this.Tracer.Warn(String.Format("Error while reading actuals of the battery"), thrownEx);

				if (!(thrownEx is InvalidOperationException))
					throw;
			}
		}


		/*
		public async Task ReadHealth()
		{
			var pack = this.Pack;
			if (pack == null)
				return;

			this.Tracer.DebugFormat("Reading battery health information of the battery at address 0x{0:X}.", this.Address);

			try
			{
				// Read health information
				var fullChargeCapacity = (await this.ReadUShortValue(SMBusCommandIds.FullChargeCapacity).ConfigureAwait(false)) / 1000f;
				var cycleCount = await this.ReadUShortValue(SMBusCommandIds.CycleCount).ConfigureAwait(false);
				var calculationPrecision = await this.ReadUShortValue(SMBusCommandIds.MaxError).ConfigureAwait(false) / 100f;

				// Read settings
				//status.RemainingCapacityAlarm = (float)await this.ReadUShortValue(SMBusCommandIds.RemainingCapacityAlarm) / 1000;
				//status.RemainingTimeAlarm = TimeSpan.FromMinutes(await this.ReadUShortValue(SMBusCommandIds.RemainingTimeAlarm));
				//status.BatteryMode = await this.ReadUShortValue(SMBusCommandIds.SerialNumber);
				//status.BatteryStatus = await this.ReadUShortValue(SMBusCommandIds.SerialNumber);

				foreach (var cell in pack.SubElements.OfType<SingleCell>())
				{
					cell.SetFullChargeCapacity(fullChargeCapacity);
					cell.SetCycleCount(cycleCount);
					cell.SetCalculationPrecision(calculationPrecision);
				}

				this.Tracer.Debug(new TraceBuilder()
					.AppendLine("The health information of the battery at address 0x{0:X} successfully read:", this.Address)
					.Indent()
						.AppendLine("Full-charge capacity: {0} mAh", pack.Health.FullChargeCapacity * 1000f)
						.AppendLine("Cycle count: {0}", pack.Health.CycleCount)
						.AppendLine("Calculation precision: {0}%", (int)(pack.Health.CalculationPrecision * 100f))
					.Trace());
			}
			catch (Exception ex)
			{
				Exception thrownEx = ex;
				if (thrownEx is AggregateException)
					thrownEx = ((AggregateException)thrownEx).Flatten().InnerException;

				this.Tracer.Warn(String.Format("Error while reading health information of the battery at address 0x{0:X}", this.Address), thrownEx);

				if (!(thrownEx is InvalidOperationException))
					throw;
			}
		}

		public async Task ReadActuals()
		{
			this.Tracer.DebugFormat("Reading battery actuals information of the battery at address 0x{0:X}.", this.Address);

			var pack = this.Pack;
			if (pack == null)
				return;

			try
			{
				//var packVoltage = (await this.ReadUShortValue(SMBusCommandIds.Voltage).ConfigureAwait(false)) / 1000f;
				var actualCurrent = (await this.ReadShortValue(SMBusCommandIds.Current).ConfigureAwait(false)) / 1000f;
				var averageCurrent = (await this.ReadShortValue(SMBusCommandIds.AverageCurrent).ConfigureAwait(false)) / 1000f;
				var temperature = (await this.ReadUShortValue(SMBusCommandIds.Temperature).ConfigureAwait(false)) / 10f;

				var remainingCapacity = (await this.ReadUShortValue(SMBusCommandIds.RemainingCapacity).ConfigureAwait(false)) / 1000f;
				var absoluteStateOfCharge = (await this.ReadUShortValue(SMBusCommandIds.AbsoluteStateOfCharge).ConfigureAwait(false)) / 100f;
				var relativeStateOfCharge = (await this.ReadUShortValue(SMBusCommandIds.RelativeStateOfCharge).ConfigureAwait(false)) / 100f;
				TimeSpan actualRunTime, averageRunTime;
				if (actualCurrent >= 0)
				{
					actualRunTime = TimeSpan.FromMinutes(await this.ReadUShortValue(SMBusCommandIds.RunTimeToEmpty).ConfigureAwait(false));
					averageRunTime = TimeSpan.FromMinutes(await this.ReadUShortValue(SMBusCommandIds.AverageTimeToEmpty).ConfigureAwait(false));
				}
				else
				{
					actualRunTime = TimeSpan.FromMinutes(await this.ReadUShortValue(SMBusCommandIds.AverageTimeToFull).ConfigureAwait(false));
					averageRunTime = actualRunTime;
				}

				//conditions.ChargingVoltage = (await this.ReadUShortValue(SMBusCommandIds.ChargingVoltage).ConfigureAwait(false)) / 1000f;
				//conditions.ChargingCurrent = (await this.ReadUShortValue(SMBusCommandIds.ChargingCurrent).ConfigureAwait(false)) / 1000f;

				var cells = pack.SubElements.OfType<SingleCell>().ToList();
				for (int i = 0; i < cells.Count; i++)
				{
					var cell = cells[i];
					//cell.BeginUpdate();
					//try
					//{

					var cellVoltage = (await this.ReadUShortValue(this.GetCellVoltageCommandId(i)).ConfigureAwait(false)) / 1000f;
					cell.SetVoltage(cellVoltage);
					cell.SetActualCurrent(actualCurrent);
					cell.SetAverageCurrent(averageCurrent);
					cell.SetTemperature(temperature);

					cell.SetRemainingCapacity(remainingCapacity);
					cell.SetAbsoluteStateOfCharge(absoluteStateOfCharge);
					cell.SetRelativeStateOfCharge(relativeStateOfCharge);
					cell.SetActualRunTime(actualRunTime);
					cell.SetAverageRunTime(averageRunTime);

					//}
					//finally
					//{
					//	conditions.EndUpdate();
					//}
				}

				var actuals = pack.Actuals;

				this.Tracer.Debug(new TraceBuilder()
					.AppendLine("The actuals of the battery at address 0x{0:X} successfully read:", this.Address)
					.Indent()
						.AppendLine("Voltage:                 {0} V ({1})", actuals.Voltage, pack.SubElements.Select((c, i) => string.Format("{0}: {1} V", i, c.Actuals.Voltage)).Join(", "))
						.AppendLine("Actual current:          {0} mA", actuals.ActualCurrent * 1000f)
						.AppendLine("Average current:         {0} mA", actuals.AverageCurrent * 1000f)
						.AppendLine("Temperature:             {0:f2} °C", actuals.Temperature - 273.15f)
						.AppendLine("Remaining capacity:      {0:N0} mAh", actuals.RemainingCapacity * 1000f)
						.AppendLine("Absolute StateOfCharge:  {0} %", actuals.AbsoluteStateOfCharge * 100f)
						.AppendLine("Relative StateOfCharge:  {0} %", actuals.RelativeStateOfCharge * 100f)
						.AppendLine("Actual run time:         {0}", actuals.ActualRunTime.ToString())
						.AppendLine("Average run time:        {0}", actuals.AverageRunTime.ToString())
					.Trace());

				
			}
			catch (Exception ex)
			{
				Exception thrownEx = ex;
				if (thrownEx is AggregateException)
					thrownEx = ((AggregateException)thrownEx).Flatten().InnerException;

				this.Tracer.Warn(String.Format("Error while reading actuals of the battery at address 0x{0:X}", this.Address), thrownEx);

				if (!(thrownEx is InvalidOperationException))
					throw;
			}
		}

		private uint GetCellVoltageCommandId(int cellIndex)
		{
			switch (cellIndex)
			{
			case 0: return SMBusCommandIds.CellVoltage1;
			case 1: return SMBusCommandIds.CellVoltage2;
			case 2: return SMBusCommandIds.CellVoltage3;
			case 3: return SMBusCommandIds.CellVoltage4;
			default: throw new ArgumentOutOfRangeException("cellIndex", cellIndex, "Only cells with index 0 to 3 are valid.");
			}
		}
		*/

		#endregion Readings


		#region Monitoring

		private const int HealthToActualsRatio = 10;
		private readonly List<UpdatesSubscription> m_subscriptions = new List<UpdatesSubscription>();
		private int m_measurementCount;

		public ISubscription SubscribeToUpdates(Action<BatteryPack> notificationConsumer, UpdateFrequency frequency = UpdateFrequency.Normal)
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
				this.m_measurementCount = 0;
				this.m_monitoringTask.Start();
				this.Tracer.InfoFormat("Monitoring of the battery started.");
			}
			else if (!hasSubscribers && this.m_monitoringTask.IsRunning)
			{
				this.m_monitoringTask.Stop();
				this.Tracer.InfoFormat("Monitoring of the battery stopped.");
			}
				

			// TODO: update periode
		}

		private void MonitoringAction()
		{
			// Read values
			if ((this.m_measurementCount++ % HealthToActualsRatio) == 0)
				this.ReadHealth().Wait();
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


		#region Descriptions

		public IEnumerable<ReadingDescriptorGrouping> GetDescriptors()
		{
			if (this.Pack == null)
				yield break;

			yield return new ReadingDescriptorGrouping(
				"Product",
				new[] {
					ReadingDescriptors.Manufacturer,
					ReadingDescriptors.Product,
					ReadingDescriptors.ManufactureDate,
					ReadingDescriptors.SerialNumber,
					ReadingDescriptors.Chemistry,
				});

			yield return new ReadingDescriptorGrouping(
				"Design",
				new[] {
					ReadingDescriptors.NominalVoltage,
					ReadingDescriptors.DesignedDischargeCurrent,
					ReadingDescriptors.MaxDischargeCurrent,
					ReadingDescriptors.DesignedCapacity,
				});

			//yield return new ReadingDescriptorGrouping(
			//	"Health",
			//	new[] {
			//		ReadingDescriptors.FullChargeCapacity,
			//		ReadingDescriptors.CycleCount,
			//		ReadingDescriptors.CalculationPrecision
			//	});

			var actualDescriptors = new List<ReadingDescriptor>();
			actualDescriptors.Add(ReadingDescriptors.PackVoltage);
			//actualDescriptors.AddRange(Enumerable.Range(0, this.Pack.SubElements.Count()).Select(SMBusReadingDescriptors.CreateCellVoltageDescriptor));
			actualDescriptors.Add(ReadingDescriptors.ActualCurrent);
			actualDescriptors.Add(ReadingDescriptors.AverageCurrent);
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
			EventHandler handlers = this.DescriptorsChanged;
			if (handlers != null)
				handlers(this, EventArgs.Empty);
		}

		#endregion Descriptions


		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Fires the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="propertyName">The name of the chnaged property.</param>
		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler handlers = this.PropertyChanged;
			if (handlers != null)
				handlers(this, new PropertyChangedEventArgs(propertyName));
		}

		private void CheckChainLength(string actionDescription, params byte[][][] chainData)
		{
			if (chainData.Any(x => x.Length != this.ChainLength))
			{
				var message = String.Format(
					"Inconsistent daisy chain length detected while {0}. Lengths detected: {1}",
					actionDescription,
					chainData.Select(x => x.Length.ToString(CultureInfo.InvariantCulture)).Join(", "));

				this.Tracer.Error(message);
				throw new InvalidOperationException(message);
			}
		}
    }
}
