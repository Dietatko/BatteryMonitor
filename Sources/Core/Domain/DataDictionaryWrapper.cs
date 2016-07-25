using System;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public abstract class DataDictionaryWrapper
	{
		protected DataDictionaryWrapper(DataDictionary data)
		{
			Contract.Requires(data, "data").IsNotNull();

			this.Data = data;
		}

		public DataDictionary Data { get; private set; }

		protected abstract string DefaultNamespaceUri { get; }

		protected virtual T GetValue<T>(string entryName)
		{
			return this.GetValue<T>(this.DefaultNamespaceUri, entryName);
		}

		protected virtual T GetValue<T>(string namespaceUri, string entryName)
		{
			return this.Data.GetValue<T>(namespaceUri, entryName);
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