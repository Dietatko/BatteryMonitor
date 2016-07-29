using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Domain.Description;

namespace ImpruvIT.BatteryMonitor.WPFApp.ViewLogic
{
	public class ReadingsGridViewLogic : ViewLogicBase
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

		public IEnumerable<ReadingDescriptorGrouping> Descriptors
		{
			get { return this.m_descriptors; }
			set
			{
				if (this.SetPropertyValue(ref this.m_descriptors, value))
					this.Bind();
			}
		}
		private IEnumerable<ReadingDescriptorGrouping> m_descriptors;

		public IEnumerable<ReadingsGroupViewLogic> Groups
		{
			get { return this.m_groups; }
			protected set
			{
				if (this.SetPropertyValue(ref this.m_groups, value))
				{
					ReadingsGroupViewLogic selectedGroup = null;
					if (this.Groups != null)
					{
						selectedGroup = this.Groups.FirstOrDefault(x => x.Group.IsDefault);
						if (selectedGroup == null)
							selectedGroup = this.Groups.FirstOrDefault();
					}

					this.SelectedGroup = selectedGroup;
				}
			}
		}
		private IEnumerable<ReadingsGroupViewLogic> m_groups;

		public ReadingsGroupViewLogic SelectedGroup
		{
			get { return this.m_selectedGroup; }
			set { this.SetPropertyValue(ref this.m_selectedGroup, value); }
		}
		private ReadingsGroupViewLogic m_selectedGroup;

		private void Bind()
		{
			if (this.Battery == null || this.Descriptors == null)
			{
				this.Groups = new ReadingsGroupViewLogic[0];
				return;
			}

			this.Groups = this.Descriptors.Select(x => new ReadingsGroupViewLogic(this.Battery, x)).ToList();
		}
	}
}
