using System;
using System.Collections.Generic;
using ImpruvIT.Contracts;

namespace ImpruvIT
{
	/// <summary>
	/// Extension methods for generic enumerations.
	/// </summary>
	public static class EnumerableExtensions
	{
		/// <summary>
		/// Concatenates the specified strings without separator.
		/// </summary>
		/// <param name="items">The strings to concat.</param>
		/// <returns>The concatenated string.</returns>
		public static string Concat(this IEnumerable<string> items)
		{
			return String.Concat(items);
		}

		/// <summary>
		/// Joins the specified strings with separators.
		/// </summary>
		/// <param name="items">The strings to concat.</param>
		/// <param name="separator">The separator used between joined strings.</param>
		/// <returns>The joined string.</returns>
		public static string Join(this IEnumerable<string> items, string separator)
		{
			return String.Join(separator, items);
		}

		/// <summary>
		/// Executes specified action for every element in the enumeration.
		/// </summary>
		/// <typeparam name="T">The type of the items.</typeparam>
		/// <param name="items">The items.</param>
		/// <param name="action">The action.</param>
		public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
		{
			Contract.Requires(items, "items").NotToBeNull();
			Contract.Requires(action, "action").NotToBeNull();

			foreach (var item in items)
				action(item);
		}
	}
}
