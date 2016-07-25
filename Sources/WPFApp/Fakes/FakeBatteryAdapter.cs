using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace ImpruvIT.BatteryMonitor.Communication
{
	public class FakeBatteryAdapter : IBatteryAdapter
	{

		public FakeBatteryAdapter()
        {
			this.Battery = new OldBattery();
        }

		private object m_lock = new Object();
		public OldBattery Battery { get; private set; }

		public void RecognizeBattery()
		{
			BatteryInformation information = this.Battery.Information;

			// Faked values
			information.Manufacturer = "Manufacturer";
			information.Product = "Product";
			information.Chemistry = "Chemistry";
			information.ManufactureDate = new DateTime(2014, 1, 1);
			information.SerialNumber = 1;
			information.CellCount = 3;
			information.NominalVoltage = 11.1f;
			//information.DesignedDischargeCurrent = this.ReadWordCommand(SMBusCommandIds.ManufactureDate);
			//information.MaxDischargeCurrent = this.ReadWordCommand(SMBusCommandIds.ManufactureDate);
			information.DesignedCapacity = 1.1f;
			information.SpecificationVersion = new Version(1, 0);
			information.VoltageScale = 1;
			information.CurrentScale = 1;
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
				this.Battery.Status.RemainingCapacityAlarm = remainingCapacity.Value;
			}

			if (remainingTime.HasValue)
			{
				this.Battery.Status.RemainingTimeAlarm = remainingTime.Value;
			}
		}

		#endregion Settings


		#region Status monitoring

		public void ReadStatus()
		{
			BatteryStatus status = this.Battery.Status;

			// Read status
			status.FullChargeCapacity = 1.05f;
			status.CycleCount = 202;
			status.MaxError = 7;

			// Read settings
			status.RemainingCapacityAlarm = 0.1f;
			status.RemainingTimeAlarm = TimeSpan.FromMinutes(1);
			//status.BatteryMode = this.ReadWordCommand(SMBusCommandIds.SerialNumber);
			//status.BatteryStatus = this.ReadWordCommand(SMBusCommandIds.SerialNumber);
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
			lock (this.m_lock)
			{
				if (this.IsMonitoringConditions)
					throw new InvalidOperationException("Conditions monitoring is already running.");

				this.m_conditionsUpdateInterval = updateInterval;

				// Start monitoring task
				this.m_conditionsMonitoringCancelSource = new CancellationTokenSource();
				this.m_conditionsMonitoringTask = new Task(
					this.ConditionsMonitoringThread,
					this.m_conditionsMonitoringCancelSource.Token,
					TaskCreationOptions.AttachedToParent | TaskCreationOptions.LongRunning);
				this.m_conditionsMonitoringTask.Start();
			}
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
				this.m_conditionsMonitoringCancelSource.Cancel();
				this.m_conditionsMonitoringTask.Wait();

				// Clean up
				this.m_conditionsMonitoringTask = null;
				this.m_conditionsMonitoringCancelSource = null;
			}
        }

		private void ConditionsMonitoringThread()
		{
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
			if (valueSelectors != null)
			{
				valueSelectors = valueSelectors.ToList();

				var valueSelector = valueSelectors.First();
				UnaryExpression body = valueSelector.Body as UnaryExpression;

				Expression<Func<BatteryConditions, object>> expected = x => x.Voltage;

				var same = valueSelector.Equals(expected);
			}

			BatteryConditions conditions = this.Battery.Conditions;

			// Read conditions
			conditions.Voltage = 10.78f;
			conditions.SetCellVoltage(0, 3.58f);
			conditions.SetCellVoltage(0, 3.61f);
			conditions.SetCellVoltage(0, 3.59f);
			conditions.SetCellVoltage(0, 0.0f);
			conditions.Current = 0.115f;
			conditions.AverageCurrent = 0.124f;
			conditions.Temperature = 22.8f;

			conditions.RemainingCapacity = 0.65f;
			conditions.AbsoluteStateOfCharge = 62;
			conditions.RelativeStateOfCharge = 64;
			conditions.RunTimeToEmpty = TimeSpan.FromMinutes(3);
			conditions.AverageTimeToEmpty = TimeSpan.FromMinutes(2);

			conditions.ChargingVoltage = 0.0f;
			conditions.ChargingCurrent = 0.0f;
			conditions.AverageTimeToFull = TimeSpan.Zero;
		}

		public event EventHandler<CurrentConditionsEventArgs> CurrentConditionsUpdated;

		protected void OnCurrentConditionsUpdated(BatteryConditions currentConditions)
		{
			EventHandler<CurrentConditionsEventArgs> handlers = this.CurrentConditionsUpdated;
			if (handlers != null)
				handlers(this, new CurrentConditionsEventArgs(currentConditions));
		}

		#endregion Conditions monitoring
    }
}
