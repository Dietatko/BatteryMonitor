using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public class SeriesPack : Pack
	{
		public SeriesPack(IEnumerable<BatteryElement> subElements)
			: base(subElements)
		{
		}
	}
}
