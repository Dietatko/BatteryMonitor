using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor
{
	// TODO: extract to Bricks
	public static class EnumerableExtensions
	{
		public static string Concat(this IEnumerable<string> items)
		{
			return String.Concat(items);
		}

		public static string Join(this IEnumerable<string> items, string separator)
		{
			return String.Join(separator, items);
		}

		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			foreach (var item in items)
				action(item);
		}
	}
}
