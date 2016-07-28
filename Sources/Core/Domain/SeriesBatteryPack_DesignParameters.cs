using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public partial class SeriesBatteryPack
	{
		private class SeriesPackDesignParameters : IDesignParameters
		{
			public SeriesPackDesignParameters(IEnumerable<BatteryElement> subElements)
			{
				this.SubElements = subElements;
			}

			private IEnumerable<BatteryElement> SubElements { get; set; }

			
			public float NominalVoltage
			{
				get { return this.SubElements.Sum(x => x.DesignParameters.NominalVoltage); }
			}

			public float DesignedDischargeCurrent
			{
				get { return this.SubElements.Min(x => x.DesignParameters.DesignedDischargeCurrent); }
			}

			public float MaxDischargeCurrent
			{
				get { return this.SubElements.Min(x => x.DesignParameters.MaxDischargeCurrent); }
			}

			public float DesignedCapacity
			{
				get { return this.SubElements.Min(x => x.DesignParameters.DesignedCapacity); }
			}
		}
	}
}
