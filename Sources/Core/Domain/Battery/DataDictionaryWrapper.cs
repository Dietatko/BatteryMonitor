using System;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public abstract class DataDictionaryWrapperBase
	{
		protected DataDictionaryWrapperBase(ReadingStorage data)
		{
			Contract.Requires(data, "data").NotToBeNull();

			this.Data = data;
		}

		public ReadingStorage Data { get; private set; }

		public T GetValue<T>(EntryKey key, T defaultValue = default(T))
		{
			T result = defaultValue;

			IReadingValue readingValue;
			if (this.Data.TryGetValue(key, out readingValue))
			{
				if (readingValue.IsDefined)
					result = readingValue.Get<T>();
			}

			return result;
		}

		public void SetValue<T>(EntryKey key, T value)
		{
			//var readingValue = this.Data.GetValue(key);
			//readingValue.Set(value);

			this.Data.SetValue(key, value);
		}
	}
}