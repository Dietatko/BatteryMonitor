using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain.Battery;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public class MathFunctionReadingValue<TValue> : ComputedReadingValue
	{
		public MathFunctionReadingValue(
			IEnumerable<BatteryElement> elements,
			EntryKey key,
			Func<IEnumerable<TValue>, TValue> getSelector)
			: base(
				elements,
				el => el.All(x => x.CustomData.IsValueDefined(key)),
				el => getSelector(el.Select(y => y.CustomData.GetValue<TValue>(key))),
				null,
				el => el.ForEach(x => x.CustomData.ResetValue(key)))
		{
		}

		public MathFunctionReadingValue(
			IEnumerable<BatteryElement> elements,
			EntryKey key,
			Func<IEnumerable<TValue>, TValue> getSelector,
			Func<BatteryElement[], TValue, TValue> setSelector)
			: base(
				elements,
				el => el.All(x => x.CustomData.IsValueDefined(key)),
				el => getSelector(el.Select(y => y.CustomData.GetValue<TValue>(key))),
				(el, value) =>
				{
					var elementValue = setSelector(el, (TValue)value);
					el.ForEach(x => x.CustomData.SetValue(key, elementValue));
				},
				el => el.ForEach(x => x.CustomData.ResetValue(key)))
		{
		}
	}
}
