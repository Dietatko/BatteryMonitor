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

		public IEnumerable<ReadingDescriptorGrouping> ReadingDescriptors
		{
			get { return this.m_readingDescriptors; }
			set { this.SetPropertyValue(ref this.m_readingDescriptors, value); }
		}
		private IEnumerable<ReadingDescriptorGrouping> m_readingDescriptors;

		protected IEnumerable<ReadingDescriptorGrouping> GetDescriptors()
		{
			yield return new ReadingDescriptorGrouping(
					"Product",
					new[] {
						ViewLogic.ReadingDescriptors.Manufacturer,
						ViewLogic.ReadingDescriptors.Product,
						ViewLogic.ReadingDescriptors.ManufactureDate,
						ViewLogic.ReadingDescriptors.SerialNumber,
						ViewLogic.ReadingDescriptors.Chemistry
						//ReadingDescriptors.Manufacturer new ReadingDescriptor<BatteryPack, Version>(b => b.Information.SpecificationVersion, "Information.SpecificationVersion", "SpecificationVersion", "The SMBus specification version the battery pack conforms to.");
						//ReadingDescriptors.Manufacturer new ReadingDescriptor<BatteryPack, object>(b => b.Information.CellCount, "Information.CellCount", "{0} cells", "Cells", "A number of cells in the battery pack.");
					});

			yield return new ReadingDescriptorGrouping(
					"Design",
					new[] {
						ViewLogic.ReadingDescriptors.NominalVoltage,
						ViewLogic.ReadingDescriptors.DesignedDischargeCurrent,
						ViewLogic.ReadingDescriptors.MaxDischargeCurrent,
						ViewLogic.ReadingDescriptors.DesignedCapacity
					});

			yield return new ReadingDescriptorGrouping(
					"Health",
					new[] {
						ViewLogic.ReadingDescriptors.FullChargeCapacity,
						ViewLogic.ReadingDescriptors.CycleCount,
						ViewLogic.ReadingDescriptors.CalculationPrecision
						////new ReadingDescriptor<BatteryPack, object>(b => b.Status.RemainingCapacityAlarm * 1000, "Status.RemainingCapacityAlarm", "{0} mAh", "Capacity alarm threshold", "A remaining capacity of the battery pack that will trigger alarm notification."),
						////new ReadingDescriptor<BatteryPack, object>(b => b.Status.RemainingTimeAlarm, "Status.RemainingTimeAlarm", "Time alarm threshold", "A remaining usage time of the battery pack that will trigger alarm notification.")
					});

			yield return new ReadingDescriptorGrouping(
					"Actuals",
					new[] {
						ViewLogic.ReadingDescriptors.PackVoltage,
						ViewLogic.ReadingDescriptors.ActualCurrent,
						ViewLogic.ReadingDescriptors.AverageCurrent,
						ViewLogic.ReadingDescriptors.Temperature
					})
				{
					IsDefault = true
				};
		}

		#endregion Descriptions
	}
}
