using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public class TypedReadingValue<TValue> : IReadingValue
	{
		public bool IsDefined { get; private set; }

		protected TValue Value { get; private set; }

		public T Get<T>()
		{
			if (!typeof(TValue).IsAssignableFrom(typeof(T)))
				throw new InvalidOperationException(String.Format("Unable to convert reading value of type '{0}' to requested type '{1}'.", typeof(TValue).FullName, typeof(T).FullName));

			if (!this.IsDefined)
				throw new KeyNotFoundException("The reading value is not defined.");

			return (T)Convert.ChangeType(this.Value, typeof(T));
		}

		public void Set(object value)
		{
			this.Value = (TValue)value;
			this.IsDefined = true;
		}

		public void Reset()
		{
			this.IsDefined = false;
			this.Value = default(TValue);
		}
	}
}
