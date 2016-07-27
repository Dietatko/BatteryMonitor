using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LiveCharts;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Protocols;

namespace ImpruvIT.BatteryMonitor.WPFApp.ViewLogic
{
	public class BatteryLogic : ViewLogicBase
	{
		public static DateTime BaseTime = DateTime.UtcNow;

		public BatteryLogic(IBatteryPackAdapter batteryAdapter)
		{
			this.ActualsHistory = new ChartValues<ActualsSnapshot>();
			this.BatteryAdapter = batteryAdapter;

			this.BatteryInformationDescriptions = new ListBase<IReadingDescription<BatteryPack, object>>(this.GetInformationDescriptions());
			this.BatteryConditionsDescriptions = new ListBase<IReadingDescription<BatteryPack, object>>(this.GetConditionsDescriptions());
		}

		protected IBatteryPackAdapter BatteryAdapter
		{
			get { return this.m_batteryAdapter; }
			private set
			{
				if (Object.ReferenceEquals(this.m_batteryAdapter, value))
					return;

				this.m_batteryAdapter = value;

				this.OnPropertyChanged("BatteryAdapter");

				this.PassThroughPropertyChangeNotification(this.m_batteryAdapter, x => Pack, () => Pack);
				this.OnPropertyChanged("Pack");
			}
		}
		private IBatteryPackAdapter m_batteryAdapter;

		public BatteryPack Pack
		{
			get { return this.BatteryAdapter.Pack; }
		}


		#region Monitoring

		private ISubscription m_updatesSubscription;

		public ChartValues<ActualsSnapshot> ActualsHistory { get; private set; }

		public Task StartMonitoring()
		{
			this.ActualsHistory.Clear();

			return this.BatteryAdapter.RecognizeBattery()
				.ContinueWith(t =>
				{
					this.m_updatesSubscription = this.m_batteryAdapter.SubscribeToUpdates(this.UpdateActuals, UpdateFrequency.Normal);
				});
		}

		public void StopMonitoring()
		{
			var subscription = Interlocked.Exchange(ref this.m_updatesSubscription, null);
			if (subscription != null)
				subscription.Unsubscribe();
		}

		private void UpdateActuals(BatteryPack pack)
		{
			if (pack == null)
				return;

			//Dispatcher.CurrentDispatcher.Invoke(() =>
			//{
			this.ActualsHistory.Add(new ActualsSnapshot(CloneActuals(pack)));

			if (this.ActualsHistory.Count > 120)
			{
				var recordsToDelete = this.ActualsHistory.OrderBy(x => x.Timestamp).Take(this.ActualsHistory.Count - 110).ToList();
				foreach (var record in recordsToDelete)
					this.ActualsHistory.Remove(record);
			}
			//});
		}

		private static Actuals CloneActuals(BatteryPack pack)
		{
			return new Actuals
				{
					PackVoltage = pack.Actuals.Voltage,
					Cell1Voltage = pack.SubElements.ElementAt(0).Actuals.Voltage,
					Cell2Voltage = pack.SubElements.ElementAt(1).Actuals.Voltage,
					Cell3Voltage = pack.SubElements.ElementAt(2).Actuals.Voltage,
					ActualCurrent = pack.Actuals.ActualCurrent,
					Capacity = pack.Actuals.RemainingCapacity,
					Temperature = pack.Actuals.Temperature
				};
		}

		#endregion Monitoring


		#region Descriptions

		public ListBase<IReadingDescription<BatteryPack, object>> BatteryInformationDescriptions
		{
			get { return this.m_batteryInformationDescriptions; }
			set
			{
				if (Object.ReferenceEquals(this.m_batteryInformationDescriptions, value))
					return;

				this.m_batteryInformationDescriptions = value;

				this.OnPropertyChanged("BatteryInformationDescriptions");
			}
		}
		private ListBase<IReadingDescription<BatteryPack, object>> m_batteryInformationDescriptions;

		protected virtual IEnumerable<IReadingDescription<BatteryPack, object>> GetInformationDescriptions()
		{
			// Product
			yield return new ReadingDescription<BatteryPack, string>(b => b.Product.Manufacturer, "Information.Manufacturer", "Manufacturer", "The manufacturer of the battery pack.");
			yield return new ReadingDescription<BatteryPack, string>(b => b.Product.Product, "Information.Product", "Product", "The battery pack product identifier.");
			yield return new ReadingDescription<BatteryPack, object>(b => b.Product.ManufactureDate, "Information.ManufactureDate", "{0:d}", "Manufacture date", "The battery pack manufacture date.");
			yield return new ReadingDescription<BatteryPack, object>(b => b.Product.SerialNumber, "Information.SerialNumber", "Serial number", "The battery pack serial number.");
			yield return new ReadingDescription<BatteryPack, string>(b => b.Product.Chemistry, "Information.Chemistry", "Chemistry", "The battery pack chemistry.");
			//yield return new ReadingDescription<BatteryPack, Version>(b => b.Information.SpecificationVersion, "Information.SpecificationVersion", "SpecificationVersion", "The SMBus specification version the battery pack conforms to.");
			//yield return new ReadingDescription<BatteryPack, object>(b => b.Information.CellCount, "Information.CellCount", "{0} cells", "Cells", "A number of cells in the battery pack.");

			// Parameters
			yield return new ReadingDescription<BatteryPack, object>(b => b.ProductionParameters.NominalVoltage, "Information.NominalVoltage", "{0} V", "Nominal voltage", "The nominal voltage of the battery pack.");
			yield return new ReadingDescription<BatteryPack, object>(b => b.ProductionParameters.DesignedDischargeCurrent, "Information.DesignedDischargeCurrent", "{0} A", "Discharge current", "A continuos discharge current of the battery pack.");
			yield return new ReadingDescription<BatteryPack, object>(b => b.ProductionParameters.MaxDischargeCurrent, "Information.MaxDischargeCurrent", "{0} A", "Max discharge current", "A maximal short-time (pulse) discharge current of the battery pack.");
			yield return new ReadingDescription<BatteryPack, object>(b => b.ProductionParameters.DesignedCapacity * 1000, "Information.DesignedCapacity", "{0} mAh", "Nominal capacity", "A designed capacity of the battery pack.");

			// Health
			yield return new ReadingDescription<BatteryPack, object>(b => b.Health.FullChargeCapacity * 1000, "Status.FullChargeCapacity", "{0} mAh", "Full charge capacity", "A capacity of the full-charged battery pack.");
			yield return new ReadingDescription<BatteryPack, object>(b => b.Health.CycleCount, "Status.CycleCount", "{0} cycles", "Cycles", "A number of charge-discharge cycles in life time of the battery pack.");
			yield return new ReadingDescription<BatteryPack, object>(b => b.Health.CalculationPrecision * 100, "Status.MaxError", "{0} %", "Value error", "A maximum value error of measured and calculated values.");
			//yield return new ReadingDescription<BatteryPack, object>(b => b.Status.RemainingCapacityAlarm * 1000, "Status.RemainingCapacityAlarm", "{0} mAh", "Capacity alarm threshold", "A remaining capacity of the battery pack that will trigger alarm notification.");
			//yield return new ReadingDescription<BatteryPack, object>(b => b.Status.RemainingTimeAlarm, "Status.RemainingTimeAlarm", "Time alarm threshold", "A remaining usage time of the battery pack that will trigger alarm notification.");
		}

		public ListBase<IReadingDescription<BatteryPack, object>> BatteryConditionsDescriptions
		{
			get { return this.m_batteryConditionsDescriptions; }
			set
			{
				if (Object.ReferenceEquals(this.m_batteryConditionsDescriptions, value))
					return;

				this.m_batteryConditionsDescriptions = value;

				this.OnPropertyChanged("BatteryConditionsDescriptions");
			}
		}
		private ListBase<IReadingDescription<BatteryPack, object>> m_batteryConditionsDescriptions;

		protected virtual IEnumerable<IReadingDescription<BatteryPack, object>> GetConditionsDescriptions()
		{
			yield return new ReadingDescription<BatteryPack, object>(b => b.Actuals.Voltage, "Conditions.Voltage", "{0} V", "Voltage", "The current battery pack voltage.");
			//yield return new ReadingDescription<BatteryPack, object>(b => b.Conditions.CellVoltages[0], "Conditions.CellVoltages[0]", "{0} V", "Cell 1 voltage", "The current voltage of the cell 1.");
			//yield return new ReadingDescription<BatteryPack, object>(b => b.Conditions.CellVoltages[1], "Conditions.CellVoltages[1]", "{0} V", "Cell 2 voltage", "The current voltage of the cell 2.");
			//yield return new ReadingDescription<BatteryPack, object>(b => b.Conditions.CellVoltages[2], "Conditions.CellVoltages[2]", "{0} V", "Cell 3 voltage", "The current voltage of the cell 3.");
			//yield return new ReadingDescription<BatteryPack, object>(b => b.Conditions.CellVoltages[3], "Conditions.CellVoltages[3]", "{0} V", "Cell 4 voltage", "The current voltage of the cell 4.");
			yield return new ReadingDescription<BatteryPack, object>(b => b.Actuals.ActualCurrent, "Conditions.Current", "{0} A", "Current", "The current load current.");
			yield return new ReadingDescription<BatteryPack, object>(b => b.Actuals.AverageCurrent, "Conditions.AverageCurrent", "{0} A", "Average current", "The average load current.");
			yield return new ReadingDescription<BatteryPack, object>(b => (int)(b.Actuals.Temperature - 273.15), "Conditions.Temperature", "{0} C", "Temperature", "The current pack temperature.");
		}

		#endregion Descriptions
	}
}
