using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public class DataDictionary
	{
		public DataDictionary()
		{
			this.Storage = new ConcurrentDictionary<EntryKey, object>();
		}

		protected ConcurrentDictionary<EntryKey, object> Storage { get; private set; }

		public bool HasValue(string namespaceUri, string entryName)
		{
			var key = new EntryKey(namespaceUri, entryName);
			return this.Storage.ContainsKey(key);
		}

		public IEnumerable<EntryKey> GetKeys(string namespaceUri = null)
		{
			IEnumerable<EntryKey> keys = this.Storage.Keys;
			if (namespaceUri != null)
				keys = keys.Where(x => x.NamespaceUri == namespaceUri);

			return keys;
		}

		public T GetValue<T>(string namespaceUri, string entryName)
		{
			var key = new EntryKey(namespaceUri, entryName);
			var value = this.Storage[key];

			return (T)Convert.ChangeType(value, typeof(T));
		}

		public bool TryGetValue<T>(string namespaceUri, string entryName, out T value)
		{
			var key = new EntryKey(namespaceUri, entryName);

			object tmpValue;
			if (!this.Storage.TryGetValue(key, out tmpValue))
			{
				value = default(T);
				return false;
			}

			value = (T)Convert.ChangeType(tmpValue, typeof(T));
			return true;
		}

		public void SetValue<T>(string namespaceUri, string entryName, T value)
		{
			var key = new EntryKey(namespaceUri, entryName);
			this.Storage[key] = value;

			this.OnValueChanged(key);
		}

		public void Merge(DataDictionary source)
		{
			Contract.Requires(source, "source").IsNotNull();

			foreach (var key in source.GetKeys())
				this.SetValue(key.NamespaceUri, key.Name, source.GetValue<object>(key.NamespaceUri, key.Name));
		}

		public event EventHandler<EntryKey> ValueChanged;

		protected virtual void OnValueChanged(EntryKey key)
		{
			var handlers = this.ValueChanged;
			if (handlers != null)
				handlers(this, key);
		}
	}
}
