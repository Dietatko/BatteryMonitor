using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public partial class SeriesBatteryPack
	{
		private class SeriesPackHealth : IBatteryHealth
		{
			public SeriesPackHealth(IEnumerable<BatteryElement> subElements)
			{
				this.SubElements = subElements;
			}

			private IEnumerable<BatteryElement> SubElements { get; set; }


			public float FullChargeCapacity
			{
				get { return this.SubElements.Min(x => x.Health.FullChargeCapacity); }
			}

			public int CycleCount
			{
				get { return this.SubElements.Max(x => x.Health.CycleCount); }
			}

			public float CalculationPrecision
			{
				get { return this.SubElements.Max(x => x.Health.CalculationPrecision); }
			}
		}
	}
}
