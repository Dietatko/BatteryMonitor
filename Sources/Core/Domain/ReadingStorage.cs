using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public class ReadingStorage
	{
		public ReadingStorage()
		{
			this.Storage = new ConcurrentDictionary<EntryKey, IReadingValue>();
		}

		protected ConcurrentDictionary<EntryKey, IReadingValue> Storage { get; private set; }

		public bool HasValue(string namespaceUri, string entryName)
		{
			return this.HasValue(new EntryKey(namespaceUri, entryName));
		}

		public bool HasValue(EntryKey key)
		{
			return this.Storage.ContainsKey(key);
		}

		public IEnumerable<EntryKey> GetKeys(string namespaceUri = null)
		{
			IEnumerable<EntryKey> keys = this.Storage.Keys;
			if (namespaceUri != null)
				keys = keys.Where(x => x.NamespaceUri == namespaceUri);

			return keys;
		}

		public void CreateValue(EntryKey key, IReadingValue readingValue)
		{
			Contract.Requires(key, "key").NotToBeNull();
			Contract.Requires(readingValue, "readingValue").NotToBeNull();

			this.Storage[key] = readingValue;
		}

		public void DeleteValue(EntryKey key)
		{
			Contract.Requires(key, "key").NotToBeNull();

			IReadingValue tmpReadingValue;
			this.Storage.TryRemove(key, out tmpReadingValue);
		}

		public bool IsValueDefined(EntryKey key)
		{
			var readingValue = this.Storage[key];

			return readingValue.IsDefined;
		}

		public T GetValue<T>(string namespaceUri, string entryName)
		{
			return this.GetValue<T>(new EntryKey(namespaceUri, entryName));
		}

		public T GetValue<T>(EntryKey key)
		{
			var readingValue = this.Storage[key];

			return readingValue.Get<T>();
		}

		public bool TryGetValue<T>(string namespaceUri, string entryName, out T value)
		{
			return this.TryGetValue(new EntryKey(namespaceUri, entryName), out value);
		}

		public bool TryGetValue<T>(EntryKey key, out T value)
		{
			IReadingValue readingValue;
			if (!this.Storage.TryGetValue(key, out readingValue))
			{
				value = default(T);
				return false;
			}

			value = readingValue.Get<T>();
			return true;
		}

		public void SetValue<T>(string namespaceUri, string entryName, T value)
		{
			this.SetValue(new EntryKey(namespaceUri, entryName), value);
		}

		public void SetValue<T>(EntryKey key, T value)
		{
			IReadingValue readingValue = this.Storage[key];
			readingValue.Set(value);

			this.OnValueChanged(key);
		}

		public void ResetValue(string namespaceUri, string entryName)
		{
			this.ResetValue(new EntryKey(namespaceUri, entryName));
		}

		public void ResetValue(EntryKey key)
		{
			IReadingValue readingValue = this.Storage[key];
			readingValue.Reset();

			this.OnValueChanged(key);
		}

		public void Merge(ReadingStorage source)
		{
			Contract.Requires(source, "source").NotToBeNull();

			foreach (var key in source.GetKeys())
				this.SetValue(key, source.GetValue<object>(key));
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
