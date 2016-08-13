﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ImpruvIT.Contracts;
using LiveCharts;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Domain.Description;
using ImpruvIT.BatteryMonitor.Protocols;

namespace ImpruvIT.BatteryMonitor.WPFApp.ViewLogic
{
	public class BatteryLogic : ViewLogicBase
	{
		public static DateTime BaseTime = DateTime.UtcNow;

		public BatteryLogic(IBatteryPackAdapter batteryAdapter)
		{
			Contract.Requires(batteryAdapter, "batteryAdapter").NotToBeNull();

			this.ActualsHistory = new ChartValues<ActualsSnapshot>();
			this.BatteryAdapter = batteryAdapter;

			this.BatteryAdapter.DescriptorsChanged += (s, a) => this.OnDescriptorsChanged();
			this.OnDescriptorsChanged();
		}

		private void OnDescriptorsChanged()
		{
			this.Descriptors = this.BatteryAdapter.GetDescriptors().ToList();
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

		public IEnumerable<ReadingDescriptorGrouping> Descriptors
		{
			get { return this.m_descriptors; }
			set { this.SetPropertyValue(ref this.m_descriptors, value); }
		}
		private IEnumerable<ReadingDescriptorGrouping> m_descriptors;


		#region Monitoring

		private ISubscription m_updatesSubscription;

		public ChartValues<ActualsSnapshot> ActualsHistory { get; private set; }

		public Task StartMonitoring()
		{
			this.ActualsHistory.Clear();

			return this.BatteryAdapter.RecognizeBattery()
				.ContinueWith(t =>
				{
					this.m_updatesSubscription = this.BatteryAdapter.SubscribeToUpdates(this.UpdateActuals, UpdateFrequency.Normal);
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

			this.ActualsHistory.Add(new ActualsSnapshot(CloneActuals(pack)));

			// Clenaup old values
			if (this.ActualsHistory.Count > 120)
			{
				var recordsToDelete = this.ActualsHistory.OrderBy(x => x.Timestamp).Take(this.ActualsHistory.Count - 110).ToList();
				foreach (var record in recordsToDelete)
					this.ActualsHistory.Remove(record);
			}
		}

		private static Actuals CloneActuals(BatteryPack pack)
		{
			pack = (BatteryPack)pack.SubElements.ElementAt(0);
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
	}
}
