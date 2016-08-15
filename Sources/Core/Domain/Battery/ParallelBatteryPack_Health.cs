using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public partial class ParallelBatteryPack
	{
		private class ParallelPackHealth : IBatteryHealth
		{
			public ParallelPackHealth(IEnumerable<BatteryElement> subElements)
			{
				this.SubElements = subElements;
			}

			private IEnumerable<BatteryElement> SubElements { get; set; }

			public float FullChargeCapacity
			{
				get { return this.SubElements.Min(x => x.Health.FullChargeCapacity) * this.SubElements.Count(); }
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
