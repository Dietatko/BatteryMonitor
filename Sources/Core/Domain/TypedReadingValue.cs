using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public class TypedReadingValue<TValue> : ReadingValueBase, IReadingValue
	{
		public TypedReadingValue(EntryKey key)
			: base(key)
		{
		}

		protected TValue Value { get; private set; }

		public bool IsDefined { get; private set; }

		public T Get<T>()
		{
			if (!this.IsDefined)
				throw new KeyNotFoundException("The reading value is not defined.");

			if (!typeof(TValue).IsAssignableFrom(typeof(T)))
				throw new InvalidOperationException(String.Format("Unable to convert reading value of type '{0}' to requested type '{1}'.", typeof(TValue).FullName, typeof(T).FullName));

			return (T)Convert.ChangeType(this.Value, typeof(T));
		}

		public void Set(object value)
		{
			this.Value = (TValue)value;
			this.IsDefined = true;

			this.OnValueChanged();
		}

		public void Reset()
		{
			this.IsDefined = false;
			this.Value = default(TValue);

			this.OnValueChanged();
		}
	}
}
