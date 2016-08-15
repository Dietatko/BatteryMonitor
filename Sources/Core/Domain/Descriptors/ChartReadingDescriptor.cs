using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

using ImpruvIT.BatteryMonitor.Domain.Battery;

namespace ImpruvIT.BatteryMonitor.Domain.Descriptors
{
	public class ChartReadingDescriptor : ReadingDescriptor
	{
		public ChartReadingDescriptor(
			ReadingDescription description, 
			IDictionary<Func<BatteryElement, BatteryElement>, EntryKey> sourceKeys, 
			ReadingValueAccessor accessor, 
			ReadingVisualizer chartVisualizer)
			: base(description, sourceKeys, accessor)
		{
			Contract.Requires(chartVisualizer, "chartVisualizer").NotToBeNull();

			this.ChartVisualizer = chartVisualizer;
		}

		public ReadingVisualizer ChartVisualizer { get; private set; }
	}
}
