using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using ImpruvIT.BatteryMonitor.Communication;

namespace ImpruvIT.BatteryMonitor.WPFApp.ViewLogic
{
	public class BatteryLogic : INotifyPropertyChanged
	{
		public static DateTime BaseTime = DateTime.UtcNow;

		public BatteryLogic(IBatteryAdapter batteryAdapter)
		{
			this.BatteryAdapter = batteryAdapter;
			this.Readings = new ObservableCollection<ConditionsRecord>();
		}

		protected IBatteryAdapter BatteryAdapter
		{
			get { return this.m_batteryAdapter; }
			set
			{
				if (Object.ReferenceEquals(this.m_batteryAdapter, value))
					return;

				if (this.m_batteryAdapter != null)
				{
					this.m_batteryAdapter.CurrentConditionsUpdated -= BatteryAdapter_CurrentConditionsUpdated;
					this.Readings.Clear();
				}

				this.m_batteryAdapter = value;

				if (this.m_batteryAdapter != null)
				{
					this.m_batteryAdapter.CurrentConditionsUpdated += BatteryAdapter_CurrentConditionsUpdated;
					this.m_batteryAdapter.RecognizeBattery();
					this.m_batteryAdapter.StartMonitoringConditions(TimeSpan.FromSeconds(1));
				}
				this.BatteryInformationDescriptions = new ListBase<IReadingDescription<OldBattery, object>>(this.GetInformationDescriptions());
				this.BatteryConditionsDescriptions = new ListBase<IReadingDescription<OldBattery, object>>(this.GetConditionsDescriptions());
				
				this.OnPropertyChanged(new PropertyChangedEventArgs("BatteryAdapter"));
			}
		}

		private IBatteryAdapter m_batteryAdapter;

		public OldBattery Battery
		{
			get { return this.BatteryAdapter.Battery; }
		}


		#region Battery information

		public ListBase<IReadingDescription<OldBattery, object>> BatteryInformationDescriptions
		{
			get { return this.m_batteryInformationDescriptions; }
			set
			{
				if (Object.ReferenceEquals(this.m_batteryInformationDescriptions, value))
					return;

				this.m_batteryInformationDescriptions = value;

				this.OnPropertyChanged(new PropertyChangedEventArgs("BatteryInformationDescriptions"));
			}
		}
		private ListBase<IReadingDescription<OldBattery, object>> m_batteryInformationDescriptions;

		protected virtual IEnumerable<IReadingDescription<OldBattery, object>> GetInformationDescriptions()
		{
			if (this.Battery == null)
				yield break;

			// Information
			yield return new ReadingDescription<OldBattery, string>(b => b.Information.Manufacturer, "Information.Manufacturer", "Manufacturer", "The manufacturer of the battery pack.");
			yield return new ReadingDescription<OldBattery, string>(b => b.Information.Product, "Information.Product", "Product", "The battery pack product identifier.");
			yield return new ReadingDescription<OldBattery, object>(b => b.Information.ManufactureDate, "Information.ManufactureDate", "{0:d}", "Manufacture date", "The battery pack manufacture date.");
			yield return new ReadingDescription<OldBattery, object>(b => b.Information.SerialNumber, "Information.SerialNumber", "Serial number", "The battery pack serial number.");
			yield return new ReadingDescription<OldBattery, string>(b => b.Information.Chemistry, "Information.Chemistry", "Chemistry", "The battery pack chemistry.");
			yield return new ReadingDescription<OldBattery, Version>(b => b.Information.SpecificationVersion, "Information.SpecificationVersion", "SpecificationVersion", "The SMBus specification version the battery pack conforms to.");
			yield return new ReadingDescription<OldBattery, object>(b => b.Information.CellCount, "Information.CellCount", "{0} cells", "Cells", "A number of cells in the battery pack.");
			yield return new ReadingDescription<OldBattery, object>(b => b.Information.NominalVoltage, "Information.NominalVoltage", "{0} V", "Nominal voltage", "The nominal voltage of the battery pack.");
			//yield return new ReadingDescription<Battery, float>(b => b.Information.DesignedDischargeCurrent, "Information.DesignedDischargeCurrent", "{0} A", "Discharge current", "A continuos discharge current of the battery pack.");
			//yield return new ReadingDescription<Battery, float>(b => b.Information.MaxDischargeCurrent, "Information.MaxDischargeCurrent", "{0} A", "Max discharge current", "A maximal short-time (pulse) discharge current of the battery pack.");
			yield return new ReadingDescription<OldBattery, object>(b => b.Information.DesignedCapacity * 1000, "Information.DesignedCapacity", "{0} mAh", "Nominal capacity", "A designed capacity of the battery pack.");

			// Status
			yield return new ReadingDescription<OldBattery, object>(b => b.Status.FullChargeCapacity * 1000, "Status.FullChargeCapacity", "{0} mAh", "Full charge capacity", "A capacity of the full-charged battery pack.");
			yield return new ReadingDescription<OldBattery, object>(b => b.Status.CycleCount, "Status.CycleCount", "{0} cycles", "Cycles", "A number of charge-discharge cycles in life time of the battery pack.");
			yield return new ReadingDescription<OldBattery, object>(b => b.Status.MaxError, "Status.MaxError", "{0} %", "Value error", "A maximum value error of measured and calculated values.");
			yield return new ReadingDescription<OldBattery, object>(b => b.Status.RemainingCapacityAlarm * 1000, "Status.RemainingCapacityAlarm", "{0} mAh", "Capacity alarm threshold", "A remaining capacity of the battery pack that will trigger alarm notification.");
			yield return new ReadingDescription<OldBattery, object>(b => b.Status.RemainingTimeAlarm, "Status.RemainingTimeAlarm", "Time alarm threshold", "A remaining usage time of the battery pack that will trigger alarm notification.");
		}

		#endregion Battery information


		#region Battery monitoring

		public ListBase<IReadingDescription<OldBattery, object>> BatteryConditionsDescriptions
		{
			get { return this.m_batteryConditionsDescriptions; }
			set
			{
				if (Object.ReferenceEquals(this.m_batteryConditionsDescriptions, value))
					return;

				this.m_batteryConditionsDescriptions = value;

				this.OnPropertyChanged(new PropertyChangedEventArgs("BatteryConditionsDescriptions"));
			}
		}
		private ListBase<IReadingDescription<OldBattery, object>> m_batteryConditionsDescriptions;

		public ObservableCollection<ConditionsRecord> Readings { get; private set; }

		protected virtual IEnumerable<IReadingDescription<OldBattery, object>> GetConditionsDescriptions()
		{
			if (this.Battery == null)
				yield break;

			yield return new ReadingDescription<OldBattery, object>(b => b.Conditions.Voltage, "Conditions.Voltage", "{0} V", "Voltage", "The current battery pack voltage.");
			yield return new ReadingDescription<OldBattery, object>(b => b.Conditions.CellVoltages[0], "Conditions.CellVoltages[0]", "{0} V", "Cell 1 voltage", "The current voltage of the cell 1.");
			yield return new ReadingDescription<OldBattery, object>(b => b.Conditions.CellVoltages[1], "Conditions.CellVoltages[1]", "{0} V", "Cell 2 voltage", "The current voltage of the cell 2.");
			yield return new ReadingDescription<OldBattery, object>(b => b.Conditions.CellVoltages[2], "Conditions.CellVoltages[2]", "{0} V", "Cell 3 voltage", "The current voltage of the cell 3.");
			yield return new ReadingDescription<OldBattery, object>(b => b.Conditions.CellVoltages[3], "Conditions.CellVoltages[3]", "{0} V", "Cell 4 voltage", "The current voltage of the cell 4.");
			yield return new ReadingDescription<OldBattery, object>(b => b.Conditions.Current, "Conditions.Current", "{0} A", "Current", "The current load current.");
			yield return new ReadingDescription<OldBattery, object>(b => b.Conditions.AverageCurrent, "Conditions.AverageCurrent", "{0} A", "Average current", "The average load current.");
			yield return new ReadingDescription<OldBattery, object>(b => (int)(b.Conditions.Temperature - 273.15), "Conditions.Temperature", "{0} C", "Temperature", "The current pack temperature.");
		}

		private void BatteryAdapter_CurrentConditionsUpdated(object sender, CurrentConditionsEventArgs currentConditionsEventArgs)
		{
			if (currentConditionsEventArgs.Conditions == null)
				return;
			
			//Dispatcher.CurrentDispatcher.Invoke(() =>
			//{
				this.Readings.Add(new ConditionsRecord((currentConditionsEventArgs.Conditions).Clone()));

				if (this.Readings.Count > 120)
				{
					var recordsToDelete = this.Readings.OrderBy(x => x.Timestamp).Take(this.Readings.Count - 110).ToList();
					foreach (var record in recordsToDelete)
						this.Readings.Remove(record);
				}
			//});
		}

		#endregion Battery monitoring


		public event PropertyChangedEventHandler PropertyChanged;
		/// <summary>
		/// Fires the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="args">The <see cref="PropertyChangedEventArgs"/> that contains the event data.</param>
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChangedEventHandler handlers = this.PropertyChanged;
			if (handlers != null)
				handlers(this, args);
		}
	}

	public class ConditionReadings
	{
		public ConditionReadings(DateTime timestamp, BatteryConditions conditions)
		{
			this.Timestamp = timestamp;
			this.Conditions = conditions;
		}

		public DateTime Timestamp { get; set; }
		public int TimestampSecs
		{
			get { return (int)(this.Timestamp - BatteryLogic.BaseTime).TotalSeconds; }
		}

		public BatteryConditions Conditions { get; set; }
	}
}
