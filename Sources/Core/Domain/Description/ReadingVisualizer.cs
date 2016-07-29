using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain.Description
{
	public class ReadingVisualizer
	{
		public ReadingVisualizer(Func<object, double> graphValueConverter)
		{
			Contract.Requires(graphValueConverter, "graphValueConverter").NotToBeNull();

			this.GraphValueConverter = graphValueConverter;
		}

		public Func<object, double> GraphValueConverter { get; private set; }
	}
}
