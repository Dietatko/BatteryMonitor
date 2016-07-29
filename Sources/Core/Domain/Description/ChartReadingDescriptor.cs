using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain.Description
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
			Contract.Requires(chartVisualizer, "chartVisualizer").IsNotNull();

			this.ChartVisualizer = chartVisualizer;
		}

		public ReadingVisualizer ChartVisualizer { get; private set; }
	}
}
