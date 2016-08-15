using System;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public abstract class DataDictionaryWrapperBase
	{
		protected DataDictionaryWrapperBase(DataDictionary data)
		{
			Contract.Requires(data, "data").NotToBeNull();

			this.Data = data;
		}

		public DataDictionary Data { get; private set; }

		protected abstract string NamespaceUri { get; }

		public T GetValue<T>(string entryName, T defaultValue = default(T))
		{
			T result;
			if (!this.Data.TryGetValue(this.NamespaceUri, entryName, out result))
				result = defaultValue;

			return result;
		}

		public void SetValue<T>(string entryName, T value)
		{
			this.Data.SetValue(this.NamespaceUri, entryName, value);
		}
	}
}