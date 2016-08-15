using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public partial class ParallelBatteryPack
	{
		private class ParallelPackDesignParameters : IDesignParameters
		{
			public ParallelPackDesignParameters(IEnumerable<BatteryElement> subElements)
			{
				this.SubElements = subElements;
			}

			private IEnumerable<BatteryElement> SubElements { get; set; }

			public float NominalVoltage
			{
				get { return this.SubElements.Select(x => x.DesignParameters.NominalVoltage).Distinct().Single(); }
			}

			public float DesignedDischargeCurrent
			{
				get { return this.SubElements.Min(x => x.DesignParameters.DesignedDischargeCurrent) * this.SubElements.Count(); }
			}

			public float MaxDischargeCurrent
			{
				get { return this.SubElements.Min(x => x.DesignParameters.MaxDischargeCurrent) * this.SubElements.Count(); }
			}

			public float DesignedCapacity
			{
				get { return this.SubElements.Sum(x => x.DesignParameters.DesignedCapacity); }
			}
		}
	}
}
