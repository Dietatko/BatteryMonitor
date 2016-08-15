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
			T result;
			if (!this.Data.TryGetValue(key, out result))
				result = defaultValue;

			return result;
		}

		public void SetValue<T>(EntryKey key, T value)
		{
			this.Data.SetValue(key, value);
		}
	}
}