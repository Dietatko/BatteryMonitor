using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor
{
	// TODO: extract to Bricks
	public static class ContractExtensions
	{
		public static ArgumentValueRequirement<T> ToBeDefinedEnumValue<T>(this ArgumentValueRequirement<T> req)
		{
			Contract.Requires(req, "req").IsNotNull();

			var argType = typeof(T);
			if (!argType.IsEnumDefined(req.Value))
				throw new ArgumentOutOfRangeException(req.ArgumentName, req.Value, "The specified enum valule is not defined in the enumeration.");

			return req;
		}

		public static string Concat(this IEnumerable<string> items)
		{
			return String.Concat(items);
		}

		public static string Join(this IEnumerable<string> items, string separator)
		{
			return String.Join(separator, items);
		}
	}
}
