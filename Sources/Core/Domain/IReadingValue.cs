using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public interface IReadingValue
	{
		bool IsDefined { get; }

		T Get<T>();
		void Set(object value);
		void Reset();
	}
}
