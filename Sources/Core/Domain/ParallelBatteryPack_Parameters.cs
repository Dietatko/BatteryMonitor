using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public partial class ParallelBatteryPack
	{
		private class ParallelPackParameters : IBatteryParameters
		{
			public ParallelPackParameters(IEnumerable<BatteryElement> subElements)
			{
				this.SubElements = subElements;
			}

			private IEnumerable<BatteryElement> SubElements { get; set; }

			public float NominalVoltage
			{
				get { return this.SubElements.Select(x => x.ProductionParameters.NominalVoltage).Distinct().Single(); }
			}

			public float DesignedDischargeCurrent
			{
				get { return this.SubElements.Min(x => x.ProductionParameters.DesignedDischargeCurrent) * this.SubElements.Count(); }
			}

			public float MaxDischargeCurrent
			{
				get { return this.SubElements.Min(x => x.ProductionParameters.MaxDischargeCurrent) * this.SubElements.Count(); }
			}

			public float DesignedCapacity
			{
				get { return this.SubElements.Sum(x => x.ProductionParameters.DesignedCapacity); }
			}
		}
	}
}
