using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ImpruvIT.Contracts;
using ImpruvIT.Diagnostics;
using ImpruvIT.Threading;

using ImpruvIT.BatteryMonitor.Domain.Battery;
using ImpruvIT.BatteryMonitor.Domain.Descriptors;
using Microsoft.Extensions.Logging;

namespace ImpruvIT.BatteryMonitor.Protocols.SMBus
{
	public class BatteryAdapter : IBatteryAdapter
	{
		private readonly ILogger<BatteryAdapter> logger;
		private const int RetryCount = 5;

		private readonly object @lock = new object();
		private readonly RepeatableTask monitoringTask;

		public BatteryAdapter(SMBusInterface connection, byte address, ILogger<BatteryAdapter> logger)
		{
			Contract.Requires(connection, "connection").NotToBeNull();
			
			this.logger = logger;
			Connection = connection;
			Address = address;
			
			//monitoringTask = new RepeatableTask(MonitoringAction, String.Format("SMBus Adapter - 0x{0:X}", address))
			//{
			//	MinTriggerTime =  TimeSpan.FromSeconds(1)
			//};
		}

		protected SMBusInterface Connection { get; }
		protected byte Address { get; }

		public BatteryPack Pack
		{
			get => pack;
			private set
			{
				if (ReferenceEquals(pack, value))
					return;

				pack = value;
				OnPropertyChanged("Pack");
			}
		}
		private BatteryPack pack;

		Pack IBatteryAdapter.Pack => Pack;


		#region Battery recognition

		public async Task RecognizeBattery()
		{
			logger.LogDebug($"Recognizing battery at address 0x{Address:X2} ...");

			try
			{
				var recognizedPack = await RecognizeGeometry().ConfigureAwait(false);
				await ReadProductData(recognizedPack);
				await ReadProtocolParams(recognizedPack);

                //await Connection.WriteWordCommand(Address, 0x0F, 0);
                //await Connection.WriteWordCommand(Address, 0x17, 50);

                Pack = recognizedPack;
				logger.LogInformation("Battery recognized at address 0x{0:X2}: {1} {2} ({3:F2} V, {4:N0} mAh).", Address, recognizedPack.ProductDefinition().Manufacturer, recognizedPack.ProductDefinition().Product, recognizedPack.DesignParameters().NominalVoltage, recognizedPack.DesignParameters().DesignedCapacity * 1000);
			}
			catch (Exception ex)
			{
				var thrownEx = ex;
				if (thrownEx is AggregateException exception)
					thrownEx = exception.Flatten().InnerException;

				logger.LogWarning($"Error while reading health information of the battery at address 0x{Address:X2}", thrownEx);

				if (!(thrownEx is InvalidOperationException))
					throw;
			}

			OnDescriptorsChanged();
        }

		private async Task<BatteryPack> RecognizeGeometry()
		{
            //var cellCount = (int)(await ReadUShortValue(SMBusCommandIds.CellCount).ConfigureAwait(false));
            var cellCount = 3;
			var nominalVoltage = (await ReadUShortValue(SMBusCommandIds.DesignVoltage).ConfigureAwait(false)) / 1000f;
			var cellVoltage = nominalVoltage / cellCount;

			var cells = Enumerable.Range(0, cellCount)
				.Select(i => new SingleCell(cellVoltage));
			var recognizedPack = new BatteryPack(cells);

			var designParams = recognizedPack.DesignParameters();
			//designParams.DesignedDischargeCurrent = (await ReadUShortValue(SMBusCommandIds.ManufactureDate).ConfigureAwait(false));
			designParams.DesignedDischargeCurrent = 0.0f;
			//designParams.MaxDischargeCurrent = (await ReadUShortValue(SMBusCommandIds.ManufactureDate).ConfigureAwait(false));
			designParams.MaxDischargeCurrent = 0.0f;
			designParams.DesignedCapacity = (await ReadUShortValue(SMBusCommandIds.DesignCapacity).ConfigureAwait(false)) / 1000f;

			var batterySMBusWrapper = recognizedPack.SMBusData();
			batterySMBusWrapper.CellCount = cellCount;

			logger.LogDebug(new TraceBuilder()
					.AppendLine($"A series battery with {cellCount} cells recognized at address 0x{Address:X2}:")
					.Indent()
						.AppendLine("Nominal voltage: {0} V (Cell voltage: {1} V)", nominalVoltage, cellVoltage)
						.AppendLine("Designed discharge current: {0} A", designParams.DesignedDischargeCurrent)
						.AppendLine("Maximal discharge current: {0} A", designParams.MaxDischargeCurrent)
						.AppendLine("Designed Capacity: {0} Ah", designParams.DesignedCapacity)
					.Trace());

			return recognizedPack;
		}

		private async Task ReadProductData(BatteryPack pack)
		{
			logger.LogDebug("Reading manufacturer data of battery at address 0x{Address:X2} ...");

			var productDefinition = pack.ProductDefinition();
			productDefinition.Manufacturer = await ReadStringValue(SMBusCommandIds.ManufacturerName, 16).ConfigureAwait(false);
			productDefinition.Product = await ReadStringValue(SMBusCommandIds.DeviceName, 16).ConfigureAwait(false);
			productDefinition.Chemistry = await ReadStringValue(SMBusCommandIds.DeviceChemistry, 5).ConfigureAwait(false);
			productDefinition.ManufactureDate = ParseDate(await ReadUShortValue(SMBusCommandIds.ManufactureDate).ConfigureAwait(false));
			var serialNumber = await ReadUShortValue(SMBusCommandIds.SerialNumber).ConfigureAwait(false);
			productDefinition.SerialNumber = serialNumber.ToString(CultureInfo.InvariantCulture);

			logger.LogDebug(new TraceBuilder()
					.AppendLine($"The manufacturer data of battery at address 0x{Address:X2}:")
					.Indent()
						.AppendLine("Manufacturer:     {0}", productDefinition.Manufacturer)
						.AppendLine("Product:          {0}", productDefinition.Product)
						.AppendLine("Chemistry:        {0}", productDefinition.Chemistry)
						.AppendLine("Manufacture date: {0}", productDefinition.ManufactureDate.ToShortDateString())
						.AppendLine("Serial number:    {0}", productDefinition.SerialNumber)
					.Trace());
		}

		private async Task ReadProtocolParams(BatteryPack pack)
		{
			logger.LogDebug($"Reading SMBus protocol parameters of the battery at address 0x{Address:X2} ...");

			var batterySMBusWrapper = pack.SMBusData();

            batterySMBusWrapper.BatteryMode = await ReadUShortValue(SMBusCommandIds.BatteryMode).ConfigureAwait(false);

            // Read SMBus specification info
            var specificationInfo = await ReadUShortValue(SMBusCommandIds.SpecificationInfo).ConfigureAwait(false);
			switch (specificationInfo & 0xFF)
			{
			case 0x11:
				batterySMBusWrapper.SpecificationVersion = new Version(1, 0);
				break;

			case 0x21:
				batterySMBusWrapper.SpecificationVersion = new Version(1, 1);
				break;

			case 0x31:
				batterySMBusWrapper.SpecificationVersion = new Version(1, 1, 1);
				break;
			}

			// Read value scales
			batterySMBusWrapper.VoltageScale = (int)Math.Pow(10, (specificationInfo >> 8) & 0x0F);
			batterySMBusWrapper.CurrentScale = (int)Math.Pow(10, (specificationInfo >> 12) & 0x0F);

			logger.LogDebug(new TraceBuilder()
				.AppendLine($"SMBus protocol parameters of the battery at address 0x{Address:X2}:")
				.Indent()
                    .AppendLine("Battery mode: {0}", Convert.ToString(batterySMBusWrapper.BatteryMode, 2).PadLeft(16, '0'))
                    .AppendLine("Specification version: {0}", batterySMBusWrapper.SpecificationVersion)
					.AppendLine("Voltage scale: {0}x", batterySMBusWrapper.VoltageScale)
					.AppendLine("Current scale: {0}x", batterySMBusWrapper.CurrentScale)
				.Trace());
		}

		#endregion Battery recognition


		#region Readings

		public async Task UpdateReadings()
		{
            await ReadHealth();
			await ReadActuals();
		}

        public async Task ReadHealth()
		{
			if (pack == null)
				return;

			logger.LogDebug($"Reading battery health information of the battery at address 0x{Address:X}.");

			try
			{
				// Read health information
				var fullChargeCapacity = (await ReadUShortValue(SMBusCommandIds.FullChargeCapacity).ConfigureAwait(false)) / 1000f;
				var cycleCount = await ReadUShortValue(SMBusCommandIds.CycleCount).ConfigureAwait(false);
				var calculationPrecision = await ReadUShortValue(SMBusCommandIds.MaxError).ConfigureAwait(false) / 100f;

				// Read settings
				//status.RemainingCapacityAlarm = (float)await ReadUShortValue(SMBusCommandIds.RemainingCapacityAlarm) / 1000;
				//status.RemainingTimeAlarm = TimeSpan.FromMinutes(await ReadUShortValue(SMBusCommandIds.RemainingTimeAlarm));
				//status.BatteryMode = await ReadUShortValue(SMBusCommandIds.SerialNumber);
				//status.BatteryStatus = await ReadUShortValue(SMBusCommandIds.SerialNumber);

				var packHealth = pack.Health();
				packHealth.FullChargeCapacity = fullChargeCapacity;
				packHealth.CycleCount = cycleCount;
                packHealth.CalculationPrecision = calculationPrecision <= 1.0 ? calculationPrecision : 0.0f;
				
				logger.LogDebug(new TraceBuilder()
					.AppendLine($"The health information of the battery at address 0x{Address:X2} successfully read:")
					.Indent()
						.AppendLine("Full-charge capacity: {0} mAh", packHealth.FullChargeCapacity * 1000f)
						.AppendLine("Cycle count: {0}", packHealth.CycleCount)
						.AppendLine("Calculation precision: {0}%", (int)(packHealth.CalculationPrecision * 100f))
					.Trace());
			}
			catch (Exception ex)
			{
				var thrownEx = ex;
				if (thrownEx is AggregateException exception)
					thrownEx = exception.Flatten().InnerException;

				logger.LogWarning($"Error while reading health information of the battery at address 0x{Address:X2}", thrownEx);

				if (!(thrownEx is InvalidOperationException))
					throw;
			}
		}

        public async Task ReadActuals()
		{
			logger.LogDebug($"Reading battery actuals information of the battery at address 0x{Address:X2}.");

			if (pack == null)
				return;

			try
			{
				var packActuals = pack.Actuals();

                packActuals.BatteryStatus = await ReadUShortValue(SMBusCommandIds.BatteryStatus).ConfigureAwait(false);

                packActuals.Voltage = (await ReadUShortValue(SMBusCommandIds.Voltage).ConfigureAwait(false)) / 1000f;
				packActuals.ActualCurrent = (await ReadShortValue(SMBusCommandIds.Current).ConfigureAwait(false)) / 1000f;
				packActuals.AverageCurrent = (await ReadShortValue(SMBusCommandIds.AverageCurrent).ConfigureAwait(false)) / 1000f;
				packActuals.Temperature = (await ReadUShortValue(SMBusCommandIds.Temperature).ConfigureAwait(false)) / 10f;

				packActuals.ChargingVoltage = (await ReadUShortValue(SMBusCommandIds.ChargingVoltage).ConfigureAwait(false)) / 1000f;
				packActuals.ChargingCurrent = (await ReadShortValue(SMBusCommandIds.ChargingCurrent).ConfigureAwait(false)) / 1000f;

				packActuals.RemainingCapacity = (await ReadUShortValue(SMBusCommandIds.RemainingCapacity).ConfigureAwait(false)) / 1000f;
				packActuals.AbsoluteStateOfCharge = (await ReadUShortValue(SMBusCommandIds.AbsoluteStateOfCharge).ConfigureAwait(false)) / 100f;
				packActuals.RelativeStateOfCharge = (await ReadUShortValue(SMBusCommandIds.RelativeStateOfCharge).ConfigureAwait(false)) / 100f;
				if (packActuals.ActualCurrent >= 0)
				{
					packActuals.ActualRunTime = TimeSpan.FromMinutes(await ReadUShortValue(SMBusCommandIds.RunTimeToEmpty).ConfigureAwait(false));
					packActuals.AverageRunTime = TimeSpan.FromMinutes(await ReadUShortValue(SMBusCommandIds.AverageTimeToEmpty).ConfigureAwait(false));
				}
				else
				{
					packActuals.ActualRunTime = TimeSpan.FromMinutes(await ReadUShortValue(SMBusCommandIds.AverageTimeToFull).ConfigureAwait(false));
					packActuals.AverageRunTime = packActuals.ActualRunTime;
				}

				//conditions.ChargingVoltage = (await ReadUShortValue(SMBusCommandIds.ChargingVoltage).ConfigureAwait(false)) / 1000f;
				//conditions.ChargingCurrent = (await ReadUShortValue(SMBusCommandIds.ChargingCurrent).ConfigureAwait(false)) / 1000f;

				var cells = pack.SubElements.OfType<Cell>().ToList();
				for (int i = 0; i < cells.Count; i++)
				{
					var cellActuals = cells[i].Actuals();

					var cellVoltage = (await ReadUShortValue(GetCellVoltageCommandId(i)).ConfigureAwait(false)) / 1000f;
					cellActuals.Voltage = cellVoltage;
				}

                var minCellVoltage = pack.SubElements.Min(x => x.Actuals().Voltage);
                var maxCellVoltage = pack.SubElements.Max(x => x.Actuals().Voltage);
				logger.LogDebug(new TraceBuilder()
					.AppendLine($"The actuals of the battery at address 0x{Address:X2} successfully read:")
					.Indent()
                        .AppendLine("Battery status:          {0}", Convert.ToString(packActuals.BatteryStatus, 2).PadLeft(16, '0'))
                        .AppendLine("Voltage:                 {0:F3} V ({1})", packActuals.Voltage, pack.SubElements.Select((c, i) => string.Format("{0}: {1:F3} V", i, c.Actuals().Voltage)).Join(", "))
                        .AppendLine("Delta voltage:           {0:D} mV ({1})", (int)((maxCellVoltage - minCellVoltage) * 1000.0f), pack.SubElements.Select((c, i) => string.Format("{0}: {1:D} mV", i, (int)((maxCellVoltage - c.Actuals().Voltage) * 1000.0f))).Join(", "))
                        .AppendLine("Actual current:          {0} mA", packActuals.ActualCurrent * 1000f)
						.AppendLine("Average current:         {0} mA", packActuals.AverageCurrent * 1000f)
						.AppendLine("Temperature:             {0:F2} °C", packActuals.Temperature - 273.15f)
                        .AppendLine("Charging voltage:        {0} V", packActuals.ChargingVoltage)
                        .AppendLine("Charging current:        {0} mA", packActuals.ChargingCurrent * 1000f)
                        .AppendLine("Remaining capacity:      {0:N0} mAh", packActuals.RemainingCapacity * 1000f)
						.AppendLine("Absolute StateOfCharge:  {0} %", packActuals.AbsoluteStateOfCharge * 100f)
						.AppendLine("Relative StateOfCharge:  {0} %", packActuals.RelativeStateOfCharge * 100f)
						.AppendLine("Actual run time:         {0}", packActuals.ActualRunTime.ToString())
						.AppendLine("Average run time:        {0}", packActuals.AverageRunTime.ToString())
					.Trace());
			}
			catch (Exception ex)
			{
				var thrownEx = ex;
				if (thrownEx is AggregateException exception)
					thrownEx = exception.Flatten().InnerException;

				logger.LogWarning($"Error while reading actuals of the battery at address 0x{Address:X2}", thrownEx);

				if (!(thrownEx is InvalidOperationException))
					throw;
			}
		}

		private byte GetCellVoltageCommandId(int cellIndex)
		{
			return cellIndex switch
			{
				0 => SMBusCommandIds.CellVoltage1,
				1 => SMBusCommandIds.CellVoltage2,
				2 => SMBusCommandIds.CellVoltage3,
				3 => SMBusCommandIds.CellVoltage4,
				_ => throw new ArgumentOutOfRangeException(nameof(cellIndex), cellIndex,
					"Only cells with index 0 to 3 are valid.")
			};
		}

		#endregion Readings


		#region Monitoring

		private const int HealthToActualsRatio = 10;
		private readonly List<UpdatesSubscription> subscriptions = new List<UpdatesSubscription>();
		private int measurementCount;

		public ISubscription SubscribeToUpdates(Action<Pack> notificationConsumer, UpdateFrequency frequency = UpdateFrequency.Normal)
		{
			lock (@lock)
			{
				var subscription = new UpdatesSubscription(notificationConsumer, frequency, UnsubscribeConsumer);
				subscriptions.Add(subscription);
				UpdateMonitoringTask();

				return subscription;
			}
		}

		private void UnsubscribeConsumer(UpdatesSubscription subscription)
		{
			lock (@lock)
			{
				subscriptions.Remove(subscription);
				UpdateMonitoringTask();
			}
		}

		private void UpdateMonitoringTask()
		{
			// Method has to be called within lock

			var hasSubscribers = subscriptions.Any();
			if (hasSubscribers && !monitoringTask.IsRunning)
			{
				measurementCount = 0;
				monitoringTask.Start();
				logger.LogInformation($"Monitoring of the battery at address 0x{Address:X2} started.");
			}
			else if (!hasSubscribers && monitoringTask.IsRunning)
			{
				monitoringTask.Stop();
				logger.LogInformation($"Monitoring of the battery at address 0x{Address:X2} stopped.");
			}
				

			// TODO: update period
		}

		private void MonitoringAction()
		{
			// Read values
			if ((measurementCount++ % HealthToActualsRatio) == 0)
				ReadHealth().Wait();
			ReadActuals().Wait();

			List<Task> tasks;
			lock (@lock)
			{
				tasks = subscriptions.Select(x => x.Consumer).Select(x => Task.Factory.StartNew(() => x(Pack))).ToList();
			}

			Task.WhenAll(tasks).ContinueWith(ConsumerErrorHandler, TaskContinuationOptions.OnlyOnFaulted);
		}

		private void ConsumerErrorHandler(Task task)
		{
			var ex = task.Exception;
			logger.LogWarning("Update notification consumer failed.", ex);

			// Exception intentionally swallowed
		}

        #endregion Monitoring


        //#region Settings

        ///// <summary>
        ///// Setup battery to report alarm when remaining capacity is lower than set values.
        ///// </summary>
        ///// <param name="remainingCapacity">Remaining capacity threshold (in Ah); <i>0</i> to disable the alarm; <i>null</i> not to change the threshold.</param>
        ///// <param name="remainingTime">Remaining time threshold; <i>zero timespan</i> to disable the alarm; <i>null</i> not to change the threshold.</param>
        //public void SetupRemainingCapacityAlarm(float? remainingCapacity, TimeSpan? remainingTime)
        //{
        //	if (remainingCapacity.HasValue)
        //	{
        //		remainingCapacity = 1000 * remainingCapacity.Value / Battery.Information.CurrentScale;
        //		Connection.WriteWordCommand(Address, SMBusCommandIds.RemainingCapacityAlarm, (ushort)remainingCapacity.Value);
        //	}

        //	if (remainingTime.HasValue)
        //	{
        //		Connection.WriteWordCommand(Address, SMBusCommandIds.RemainingTimeAlarm, (ushort)remainingTime.Value.TotalMinutes);
        //	}

        //	logger.LogInformationFormat(
        //		"Alarm settings for battery at {0:X} to {1} mAh or {2} remaining time.", 
        //		Address,
        //		(remainingCapacity.HasValue ? (remainingCapacity.Value * 1000).ToString("N0") : "<No change>"),
        //		(remainingTime.HasValue ? remainingTime.Value.ToString() : "<No change>"));
        //}

        //#endregion Settings


        #region Reading primitives

        public Task<byte> ReadByteValue(byte commandId)
        {
            return RetryOperation(() => Connection.ReadByteCommand(Address, commandId));
        }

        private Task<short> ReadShortValue(byte commandId)
		{
			return ReadUShortValue(commandId)
				.ContinueWith(t =>
				{
					ushort value = t.Result;
					var bytes = new byte[2];
					bytes[0] = (byte)(value & 0xFF);
					bytes[1] = (byte)((value >> 8) & 0xFF);

					return BitConverter.ToInt16(bytes, 0);
				}, TaskContinuationOptions.OnlyOnRanToCompletion);
		}

        private Task<ushort> ReadUShortValue(byte commandId)
		{
			return RetryOperation(() => Connection.ReadWordCommand(Address, commandId));
		}

		private Task<string> ReadStringValue(byte commandId, int stringLength)
		{
			return ReadBlockCommand(commandId, stringLength)
				.ContinueWith(t =>
				{
					var bytesCount = t.Result.TakeWhile(b => b != (byte)0).Count();
					return Encoding.ASCII.GetString(t.Result, 0, bytesCount);
				}, 
				TaskContinuationOptions.OnlyOnRanToCompletion);
		}

		private Task<byte[]> ReadBlockCommand(byte commandId, int blockSize)
		{
			return RetryOperation(() => Connection.ReadBlockCommand(Address, commandId, blockSize));
		}

		private async Task<T> RetryOperation<T>(Func<Task<T>> action, int retryCount = RetryCount)
		{
			int retry = 0;
			while (true)
			{
				var task = action();
				try
				{
					T value = await task;
					return value;
				}
                catch (CommunicationException)
				{
					if (retry == retryCount)
						throw;

					retry++;
                    await Task.Delay(20);
				}
			}
		}

		private static DateTime ParseDate(ushort packedValue)
		{
			var day = packedValue & 0x1F;
			var month = (packedValue >> 5) & 0x0F;
			var year = 1980 + ((packedValue >> 9) & 0x7F);

			if (day < 1 || day > 31 || month < 1 || month > 12)
				return default;

			return new DateTime(year, month, day);
		}

		#endregion Reading primitives


		#region Descriptions

		public IEnumerable<ReadingDescriptorGrouping> GetDescriptors()
		{
			if (Pack == null)
				yield break;

			yield return new ReadingDescriptorGrouping(
				"Product",
				new[] {
					ReadingDescriptors.Manufacturer,
					ReadingDescriptors.Product,
					ReadingDescriptors.ManufactureDate,
					ReadingDescriptors.SerialNumber,
					ReadingDescriptors.Chemistry,
					SMBusReadingDescriptors.SpecificationVersion
				});

			yield return new ReadingDescriptorGrouping(
				"Design",
				new[] {
					SMBusReadingDescriptors.CellCount,
					ReadingDescriptors.NominalVoltage,
					ReadingDescriptors.DesignedDischargeCurrent,
					ReadingDescriptors.MaxDischargeCurrent,
					ReadingDescriptors.DesignedCapacity,
				});

			yield return new ReadingDescriptorGrouping(
				"Health",
				new[] {
					ReadingDescriptors.FullChargeCapacity,
					ReadingDescriptors.CycleCount,
					ReadingDescriptors.CalculationPrecision
					////new ReadingDescriptor<Pack, object>(b => b.Status.RemainingCapacityAlarm * 1000, "Status.RemainingCapacityAlarm", "{0} mAh", "Capacity alarm threshold", "A remaining capacity of the battery pack that will trigger alarm notification."),
					////new ReadingDescriptor<Pack, object>(b => b.Status.RemainingTimeAlarm, "Status.RemainingTimeAlarm", "Time alarm threshold", "A remaining usage time of the battery pack that will trigger alarm notification.")
				});

			var actualDescriptors = new List<ReadingDescriptor>();
			actualDescriptors.Add(ReadingDescriptors.PackVoltage);
			actualDescriptors.AddRange(Enumerable.Range(0, Pack.ElementCount).Select(SMBusReadingDescriptors.CreateCellVoltageDescriptor));
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
			var handlers = DescriptorsChanged;
			handlers?.Invoke(this, EventArgs.Empty);
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
			var handlers = PropertyChanged;
			handlers?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
    }
}
