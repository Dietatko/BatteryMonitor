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

			this.ReadingDescriptors = this.GetDescriptors().ToList();
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

		public IEnumerable<ReadingDescriptor> ReadingDescriptors
		{
			get { return this.m_readingDescriptors; }
			set { this.SetPropertyValue(ref this.m_readingDescriptors, value); }
		}
		private IEnumerable<ReadingDescriptor> m_readingDescriptors;

		protected IEnumerable<ReadingDescriptor> GetDescriptors()
		{
			// Product
			yield return ViewLogic.ReadingDescriptors.Manufacturer;
			yield return ViewLogic.ReadingDescriptors.Product;
			yield return ViewLogic.ReadingDescriptors.ManufactureDate;
			yield return ViewLogic.ReadingDescriptors.SerialNumber;
			yield return ViewLogic.ReadingDescriptors.Chemistry;
			//yield return ReadingDescriptors.Manufacturer new ReadingDescriptor<BatteryPack, Version>(b => b.Information.SpecificationVersion, "Information.SpecificationVersion", "SpecificationVersion", "The SMBus specification version the battery pack conforms to.");
			//yield return ReadingDescriptors.Manufacturer new ReadingDescriptor<BatteryPack, object>(b => b.Information.CellCount, "Information.CellCount", "{0} cells", "Cells", "A number of cells in the battery pack.");

			// Design parameters
			yield return ViewLogic.ReadingDescriptors.NominalVoltage;
			yield return ViewLogic.ReadingDescriptors.DesignedDischargeCurrent;
			yield return ViewLogic.ReadingDescriptors.MaxDischargeCurrent;
			yield return ViewLogic.ReadingDescriptors.DesignedCapacity;

			// Health
			yield return ViewLogic.ReadingDescriptors.FullChargeCapacity;
			yield return ViewLogic.ReadingDescriptors.CycleCount;
			yield return ViewLogic.ReadingDescriptors.CalculationPrecision;
			////yield return new ReadingDescriptor<BatteryPack, object>(b => b.Status.RemainingCapacityAlarm * 1000, "Status.RemainingCapacityAlarm", "{0} mAh", "Capacity alarm threshold", "A remaining capacity of the battery pack that will trigger alarm notification.");
			////yield return new ReadingDescriptor<BatteryPack, object>(b => b.Status.RemainingTimeAlarm, "Status.RemainingTimeAlarm", "Time alarm threshold", "A remaining usage time of the battery pack that will trigger alarm notification.");

			// Actuals
			yield return ViewLogic.ReadingDescriptors.PackVoltage;
			yield return ViewLogic.ReadingDescriptors.ActualCurrent;
			yield return ViewLogic.ReadingDescriptors.AverageCurrent;
			yield return ViewLogic.ReadingDescriptors.Temperature;
		}

		#endregion Descriptions
	}
}
