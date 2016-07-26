using System;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public abstract class DataDictionaryWrapperBase
	{
		protected DataDictionaryWrapperBase(DataDictionary data)
		{
			Contract.Requires(data, "data").IsNotNull();

			this.Data = data;
		}

		public DataDictionary Data { get; private set; }

		protected abstract string DefaultNamespaceUri { get; }

		protected virtual T GetValue<T>(string entryName, T defaultValue = default(T))
		{
			return this.GetValue<T>(this.DefaultNamespaceUri, entryName, defaultValue);
		}

		protected virtual T GetValue<T>(string namespaceUri, string entryName, T defaultValue = default(T))
		{
			T result;
			if (!this.Data.TryGetValue(namespaceUri, entryName, out result))
				result = defaultValue;

			return result;
		}

		protected virtual void SetValue<T>(string entryName, T value)
		{
			this.SetValue(this.DefaultNamespaceUri, entryName, value);
		}

		protected virtual void SetValue<T>(string namespaceUri, string entryName, T value)
		{
			this.Data.SetValue(namespaceUri, entryName, value);
		}
	}
}