using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public partial class ParallelBatteryPack
	{
		private class ParallelPackActuals : IBatteryActuals
		{
			public ParallelPackActuals(IEnumerable<BatteryElement> subElements)
			{
				this.SubElements = subElements;
			}

			private IEnumerable<BatteryElement> SubElements { get; set; }


			public float Voltage
			{
				get { return this.SubElements.Select(x => x.Actuals.Voltage).Distinct().Single(); }
			}

			public float ActualCurrent
			{
				get { return this.SubElements.Sum(x => x.Actuals.ActualCurrent); }
			}

			public float AverageCurrent
			{
				get { return this.SubElements.Sum(x => x.Actuals.AverageCurrent); }
			}

			public float Temperature
			{
				get { return this.SubElements.Max(x => x.Actuals.Temperature); }
			}


			public float RemainingCapacity
			{
				get { return this.SubElements.Min(x => x.Actuals.RemainingCapacity) * this.SubElements.Count(); }
			}

			public float AbsoluteStateOfCharge
			{
				get { return this.SubElements.Average(x => x.Actuals.AbsoluteStateOfCharge); }
			}

			public float RelativeStateOfCharge
			{
				get { return this.SubElements.Average(x => x.Actuals.RelativeStateOfCharge); }
			}


			public TimeSpan ActualRunTime
			{
				get { return this.SubElements.Min(x => x.Actuals.ActualRunTime); }
			}

			public TimeSpan AverageRunTime
			{
				get { return this.SubElements.Min(x => x.Actuals.AverageRunTime); }
			}
		}
	}
}
