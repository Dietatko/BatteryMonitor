using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain.Description
{
	public class ReadingValueAccessor
	{
		private const string DefaultFormatString = "{0}";

		public ReadingValueAccessor(Func<BatteryElement, object> valueSelector, string formatString = DefaultFormatString)
		{
			Contract.Requires(valueSelector, "valueSelector").NotToBeNull();
			Contract.Requires(formatString, "formatString").NotToBeNull();

			this.ValueSelector = valueSelector;
			this.FormatString = formatString;
		}

		public Func<BatteryElement, object> ValueSelector { get; private set; }
		public string FormatString { get; private set; }
	}
}
