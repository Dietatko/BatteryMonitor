using System;
using System.Collections.Generic;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public static class Unit
	{
		public const float Epsilon = 0.000001f;
		private static readonly Dictionary<int, string> MetricPrefixes = new Dictionary<int, string>
			{
				{ 12, "T" },
				{ 9, "G" },
				{ 6, "M" },
				{ 3, "k" },
				{ 0, "" },
				{ -3, "m" },
				{ -6, "μ" },
				{ -9, "n" },
				{ -12, "p" },
			};

		public static string ToString(float value, string unit, int precision, int minPower = -12, int maxPower = 12)
		{
			Contract.Requires(unit, "unit").NotToBeNull();
			Contract.Requires(precision, "precision").ToBeInRange(x => 0 <= x);
			minPower = Math.Min(0, Math.Max(minPower, -12));
			maxPower = Math.Max(0, Math.Min(maxPower, 12));

			float tmp = value;
			int power = 0;
			if (tmp >= 1)
			{
				while (power < maxPower && tmp >= 1000f)
				{
					tmp /= 1000f;
					power += 3;
				}
			}
			else
			{
				while (minPower < power && tmp <= 0.001f)
				{
					tmp *= 1000f;
					power -= 3;
				}
			}

			var prefix = MetricPrefixes[power];
			return String.Format("{0} {1}{2}", tmp.ToString("F" + precision), prefix, unit);
		}
	}
}
