using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public partial class SeriesBatteryPack
	{
		private class SeriesPackParameters : IBatteryParameters
		{
			public SeriesPackParameters(IEnumerable<BatteryElement> subElements)
			{
				this.SubElements = subElements;
			}

			private IEnumerable<BatteryElement> SubElements { get; set; }

			
			public float NominalVoltage
			{
				get { return this.SubElements.Sum(x => x.ProductionParameters.NominalVoltage); }
			}

			public float DesignedDischargeCurrent
			{
				get { return this.SubElements.Min(x => x.ProductionParameters.DesignedDischargeCurrent); }
			}

			public float MaxDischargeCurrent
			{
				get { return this.SubElements.Min(x => x.ProductionParameters.MaxDischargeCurrent); }
			}

			public float DesignedCapacity
			{
				get { return this.SubElements.Min(x => x.ProductionParameters.DesignedCapacity); }
			}
		}
	}
}
