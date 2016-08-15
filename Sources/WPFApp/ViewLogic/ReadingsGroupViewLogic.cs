using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

using ImpruvIT.BatteryMonitor.Domain.Battery;
using ImpruvIT.BatteryMonitor.Domain.Descriptors;

namespace ImpruvIT.BatteryMonitor.WPFApp.ViewLogic
{
	public class ReadingsGroupViewLogic : ViewLogicBase
	{
		public ReadingsGroupViewLogic(BatteryElement battery, ReadingDescriptorGrouping group)
		{
			Contract.Requires(battery, "battery").NotToBeNull();
			Contract.Requires(group, "group").NotToBeNull();

			this.Battery = battery;
			this.Group = group;

			this.Readings = this.Group.Descriptors.Select(x => new BatteryReadingProvider(this.Battery, x)).ToList();
		}

		public BatteryElement Battery { get; private set; }
		public ReadingDescriptorGrouping Group { get; private set; }

		public string Title
		{
			get { return this.Group.Title; }
		}

		public IEnumerable<BatteryReadingProvider> Readings
		{
			get { return this.m_readings; }
			protected set { this.SetPropertyValue(ref this.m_readings, value); }
		}
		private IEnumerable<BatteryReadingProvider> m_readings;

		public BatteryReadingProvider SelectedReading
		{
			get { return this.m_selectedReading; }
			set { this.SetPropertyValue(ref this.m_selectedReading, value); }
		}
		private BatteryReadingProvider m_selectedReading;
	}
}
