using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using ImpruvIT.Contracts;
using log4net;

using ImpruvIT.BatteryMonitor.Protocols.SMBus;

namespace ImpruvIT.BatteryMonitor.Communication
{
	public class BatteryAdapter : IBatteryAdapter
	{
		private const int RetryCount = 3;
		private readonly object m_lock = new object();

		public BatteryAdapter(SMBusInterface connection, uint address)
        {
			this.Tracer = LogManager.GetLogger(this.GetType());

			Contract.Requires(connection, "connection").IsNotNull();
			
			this.Connection = connection;
			this.Address = address;
			this.Battery = new Battery();
        }

	    protected ILog Tracer { get; private set; }
		protected SMBusInterface Connection { get; private set; }
		protected uint Address { get; private set; }
		public Battery Battery { get; private set; }

		public void RecognizeBattery()
		{
			this.Tracer.DebugFormat("Recognizing battery at address 0x{0:X} ...", this.Address);

			BatteryInformation information = this.Battery.Information;

			// Read battery information
			information.Manufacturer = Encoding.ASCII.GetString(this.ReadBlockCommand(SMBusCommandIds.ManufacturerName, 16));
			information.Product = Encoding.ASCII.GetString(this.ReadBlockCommand(SMBusCommandIds.DeviceName, 16));
			information.Chemistry = Encoding.ASCII.GetString(this.ReadBlockCommand(SMBusCommandIds.DeviceChemistry, 5));
			information.ManufactureDate = ParseDate(this.ReadUShortCommand(SMBusCommandIds.ManufactureDate));
			information.SerialNumber = this.ReadUShortCommand(SMBusCommandIds.SerialNumber);
			//information.CellCount = this.ReadUShortCommand(SMBusCommandIds.CellCount);
			information.NominalVoltage = (float)this.ReadUShortCommand(SMBusCommandIds.DesignVoltage) / 1000;
			//information.DesignedDischargeCurrent = this.ReadUShortCommand(SMBusCommandIds.ManufactureDate);
			//information.MaxDischargeCurrent = this.ReadUShortCommand(SMBusCommandIds.ManufactureDate);
			information.DesignedCapacity = (float)this.ReadUShortCommand(SMBusCommandIds.DesignCapacity) / 1000;

			var specificationInfo = this.ReadUShortCommand(SMBusCommandIds.SpecificationInfo);
			switch (specificationInfo & 0xFF)
			{
			case 0x11:
				information.SpecificationVersion = new Version(1, 0);
				break;

			case 0x21:
				information.SpecificationVersion = new Version(1, 1);
				break;

			case 0x31:
				information.SpecificationVersion = new Version(1, 1, 1);
				break;
			}
			information.VoltageScale = (int)Math.Pow(10, (specificationInfo >> 8) & 0x0F);
			information.CurrentScale = (int)Math.Pow(10, (specificationInfo >> 12) & 0x0F);

			this.Tracer.InfoFormat("Battery recognized at address 0x{0:X}: {1} {2} ({3:F2} V, {4:N0} mAh).", this.Address, information.Manufacturer, information.Product, information.NominalVoltage, information.DesignedCapacity * 1000);
        }


		#region Settings

		/// <summary>
		/// Setup battery to report alarm when remaining capacity is lower than set values.
		/// </summary>
		/// <param name="remainingCapacity">Remaining capacity treshold (in Ah); <i>0</i> to disable the alarm; <i>null</i> not to change the threshold.</param>
		/// <param name="remainingTime">Remaining time treshold; <i>zero timespan</i> to disable the alarm; <i>null</i> not to change the threshold.</param>
		public void SetupRemainingCapacityAlarm(float? remainingCapacity, TimeSpan? remainingTime)
		{
			if (remainingCapacity.HasValue)
			{
				remainingCapacity = 1000 * remainingCapacity.Value / this.Battery.Information.CurrentScale;
				this.Connection.WriteWordCommand(this.Address, SMBusCommandIds.RemainingCapacityAlarm, (ushort)remainingCapacity.Value);
			}

			if (remainingTime.HasValue)
			{
				this.Connection.WriteWordCommand(this.Address, SMBusCommandIds.RemainingTimeAlarm, (ushort)remainingTime.Value.TotalMinutes);
			}

			this.Tracer.InfoFormat(
				"Alarm settings for battery at {0:X} to {1} mAh or {2} remaining time.", 
				this.Address,
				(remainingCapacity.HasValue ? (remainingCapacity.Value * 1000).ToString("N0") : "<No change>"),
				(remainingTime.HasValue ? remainingTime.Value.ToString() : "<No change>"));
		}

		#endregion Settings


		#region Status monitoring

		public void ReadStatus()
		{
			BatteryStatus status = this.Battery.Status;

			// Read status
			status.FullChargeCapacity = (float)this.ReadUShortCommand(SMBusCommandIds.FullChargeCapacity) / 1000;
			status.CycleCount = this.ReadUShortCommand(SMBusCommandIds.CycleCount);
			status.MaxError = this.ReadUShortCommand(SMBusCommandIds.MaxError);

			// Read settings
			status.RemainingCapacityAlarm = (float)this.ReadUShortCommand(SMBusCommandIds.RemainingCapacityAlarm) / 1000;
			status.RemainingTimeAlarm = TimeSpan.FromMinutes(this.ReadUShortCommand(SMBusCommandIds.RemainingTimeAlarm));
			//status.BatteryMode = this.ReadUShortCommand(SMBusCommandIds.SerialNumber);
			//status.BatteryStatus = this.ReadUShortCommand(SMBusCommandIds.SerialNumber);
        }

		#endregion Status monitoring


		#region Conditions monitoring

	    private TimeSpan m_conditionsUpdateInterval;
	    private Task m_conditionsMonitoringTask;
		private CancellationTokenSource m_conditionsMonitoringCancelSource;

		public bool IsMonitoringConditions
        {
			get { return this.m_conditionsMonitoringTask != null; }
        }

        public void StartMonitoringConditions(TimeSpan updateInterval)
        {
			lock (m_lock)
			{
				if (this.IsMonitoringConditions)
					throw new InvalidOperationException("Conditions monitoring is already running.");

				this.m_conditionsUpdateInterval = updateInterval;

				// Start monitoring task
				this.m_conditionsMonitoringCancelSource = new CancellationTokenSource();
				this.m_conditionsMonitoringTask = new Task(
					this.ConditionsMonitoringThread,
					SynchronizationContext.Current,
					this.m_conditionsMonitoringCancelSource.Token,
					TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning);
				this.m_conditionsMonitoringTask.Start();
			}

			this.Tracer.InfoFormat("Conditions monitoring started for battery at address 0x{0:X} with update interval '{1}'.", this.Address, this.m_conditionsUpdateInterval);
        }

        public void StopMonitoringConditions()
        {
			lock (this.m_lock)
			{
				if (!this.IsMonitoringConditions)
				{
					throw new InvalidOperationException("Conditions monitoring is not running.");
				}

				// Cancel monitoring
				this.Tracer.DebugFormat("Cancelling conditions monitoring for battery at address 0x{0:X} ...", this.Address);
				this.m_conditionsMonitoringCancelSource.Cancel();
				this.m_conditionsMonitoringTask.Wait();

				// Clean up
				this.m_conditionsMonitoringTask = null;
				this.m_conditionsMonitoringCancelSource = null;
			}

			this.Tracer.InfoFormat("Conditions monitoring for battery at address 0x{0:X} stopped.", this.Address);
        }

		private void ConditionsMonitoringThread(object state)
		{
			SynchronizationContext.SetSynchronizationContext((SynchronizationContext)state);

			do
			{
				// Read values
				this.ReadActualConditions();

				// Notify
				this.OnCurrentConditionsUpdated(this.Battery.Conditions);

			} while (!this.m_conditionsMonitoringCancelSource.Token.WaitHandle.WaitOne(this.m_conditionsUpdateInterval));
		}

		public void ReadActualConditions(params Expression<Func<BatteryConditions, object>>[] valueSelectors)
		{
			this.ReadActualConditions((IEnumerable<Expression<Func<BatteryConditions, object>>>)valueSelectors);
		}

		public void ReadActualConditions(IEnumerable<Expression<Func<BatteryConditions, object>>> valueSelectors = null)
		{
			this.Tracer.DebugFormat("Reading actual conditions of battery at address 0x{0:X}.", this.Address);

			if (valueSelectors != null)
			{
				valueSelectors = valueSelectors.ToList();

				var valueSelector = valueSelectors.First();
				UnaryExpression body = valueSelector.Body as UnaryExpression;

				Expression<Func<BatteryConditions, object>> expected = x => x.Voltage;

				var same = valueSelector.Equals(expected);
			}

			BatteryConditions conditions = this.Battery.Conditions;

			// Read status
			conditions.BeginUpdate();
			try
			{
				conditions.Voltage = (float)this.ReadUShortCommand(SMBusCommandIds.Voltage) / 1000;
				conditions.SetCellVoltage(0, (float)this.ReadUShortCommand(SMBusCommandIds.CellVoltage1) / 1000);
				conditions.SetCellVoltage(1, (float)this.ReadUShortCommand(SMBusCommandIds.CellVoltage2) / 1000);
				conditions.SetCellVoltage(2, (float)this.ReadUShortCommand(SMBusCommandIds.CellVoltage3) / 1000);
				conditions.SetCellVoltage(3, (float)this.ReadUShortCommand(SMBusCommandIds.CellVoltage4) / 1000);
				conditions.Current = (float)this.ReadShortCommand(SMBusCommandIds.Current) / 1000;
				conditions.AverageCurrent = (float)this.ReadShortCommand(SMBusCommandIds.AverageCurrent) / 1000;
				conditions.Temperature = (float)this.ReadUShortCommand(SMBusCommandIds.Temperature) / 10;

				conditions.RemainingCapacity = (float)this.ReadUShortCommand(SMBusCommandIds.RemainingCapacity) / 1000;
				conditions.AbsoluteStateOfCharge = this.ReadUShortCommand(SMBusCommandIds.AbsoluteStateOfCharge);
				conditions.RelativeStateOfCharge = this.ReadUShortCommand(SMBusCommandIds.RelativeStateOfCharge);
				conditions.RunTimeToEmpty = TimeSpan.FromMinutes(this.ReadUShortCommand(SMBusCommandIds.RunTimeToEmpty));
				conditions.AverageTimeToEmpty = TimeSpan.FromMinutes(this.ReadUShortCommand(SMBusCommandIds.AverageTimeToEmpty));

				conditions.ChargingVoltage = (float)this.ReadUShortCommand(SMBusCommandIds.ChargingVoltage) / 1000;
				conditions.ChargingCurrent = (float)this.ReadUShortCommand(SMBusCommandIds.ChargingCurrent) / 1000;
				conditions.AverageTimeToFull = TimeSpan.FromMinutes(this.ReadUShortCommand(SMBusCommandIds.AverageTimeToFull));
			}
			finally
			{
				conditions.EndUpdate();
			}
		}

		public event EventHandler<CurrentConditionsEventArgs> CurrentConditionsUpdated;

		protected void OnCurrentConditionsUpdated(BatteryConditions currentConditions)
        {
            EventHandler<CurrentConditionsEventArgs> handlers = this.CurrentConditionsUpdated;
            if (handlers != null)
                handlers(this, new CurrentConditionsEventArgs(currentConditions));
        }

		#endregion Conditions monitoring

		private short ReadShortCommand(uint commandId)
		{
			ushort value = this.ReadUShortCommand(commandId);
			byte[] bytes = new byte[2];
			bytes[0] = (byte)(value & 0xFF);
			bytes[1] = (byte)((value >> 8) & 0xFF);

			return BitConverter.ToInt16(bytes, 0);
		}

		private ushort ReadUShortCommand(uint commandId)
		{
			return this.RetryOperation(() => this.Connection.ReadWordCommand(this.Address, commandId).Result);
		}

		private byte[] ReadBlockCommand(uint commandId, int blockSize)
		{
			return this.RetryOperation(() => this.Connection.ReadBlockCommand(this.Address, commandId, blockSize).Result);
		}

		private T RetryOperation<T>(Func<T> action, int retryCount = RetryCount)
		{
			int retry = 0;
			while (true)
			{
				try
				{
					return action();
				}
				catch (Exception ex)
				{
					if (ex is InvalidOperationException
						|| (ex is AggregateException && ((AggregateException)ex).InnerExceptions.OfType<InvalidOperationException>().Any()))
					{
						if (retry == retryCount)
							throw;

						retry++;
					}
					else
						throw;
				}
			}
		}

	    private static DateTime ParseDate(ushort packedValue)
	    {
		    int day = packedValue & 0x1F;
		    int month = (packedValue >> 5) & 0x0F;
		    int year = 1980 + ((packedValue >> 9) & 0x7F);

		    if (day < 1 || day > 31 || month < 1 || month > 12)
			    return default(DateTime);
			
		    return new DateTime(year, month, day);
	    }
    }
}
