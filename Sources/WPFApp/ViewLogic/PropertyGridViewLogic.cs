using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain;

namespace ImpruvIT.BatteryMonitor.WPFApp.ViewLogic
{
	public class PropertyGridViewLogic : ViewLogicBase
	{
		public BatteryElement Battery
		{
			get { return this.m_battery; }
			set
			{
				if (this.SetPropertyValue(ref this.m_battery, value))
					this.Bind();
			}
		}
		private BatteryElement m_battery;

		public IEnumerable<ReadingDescriptor> Descriptors
		{
			get { return this.m_descriptors; }
			set
			{
				if (this.SetPropertyValue(ref this.m_descriptors, value))
					this.Bind();
			}
		}
		private IEnumerable<ReadingDescriptor> m_descriptors;

		public IEnumerable<BatteryReadingProvider> Values
		{
			get { return this.m_values; }
			protected set { this.SetPropertyValue(ref this.m_values, value); }
		}
		private IEnumerable<BatteryReadingProvider> m_values;

		private void Bind()
		{
			if (this.Battery == null || this.Descriptors == null)
			{
				this.Values = new BatteryReadingProvider[0];
				return;
			}

			this.Values = this.Descriptors.Select(x => new BatteryReadingProvider(this.Battery, x));
		}
	}
}
