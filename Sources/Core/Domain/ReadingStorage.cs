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

		public void CreateValue(IReadingValue readingValue)
		{
			Contract.Requires(readingValue, "readingValue").NotToBeNull();

			this.Storage[readingValue.Key] = readingValue;
			readingValue.ValueChanged += this.OnReadingValueChanged;
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

			return keys.ToList();
		}

		public IEnumerable<IReadingValue> GetValues(string namespaceUri = null)
		{
			IEnumerable<IReadingValue> values = this.Storage.Values;
			if (namespaceUri != null)
				values = values.Where(x => x.Key.NamespaceUri == namespaceUri);

			return values.ToList();
		}

		public IReadingValue GetValue(EntryKey key)
		{
			return this.Storage[key];
		}

		public bool TryGetValue(EntryKey key, out IReadingValue value)
		{
			return this.Storage.TryGetValue(key, out value);
		}

		public void Merge(ReadingStorage source)
		{
			Contract.Requires(source, "source").NotToBeNull();

			foreach (var readingValue in source.GetValues())
			{
				this.Storage.AddOrUpdate(
					readingValue.Key, 
					readingValue,
					(key, oldVal) =>
					{
						oldVal.ValueChanged -= this.OnReadingValueChanged;
						return readingValue;
					});
				readingValue.ValueChanged += this.OnReadingValueChanged;
			}
		}

		private void OnReadingValueChanged(object sender, EventArgs args)
		{
			var readingValue = (IReadingValue)sender;
			this.OnValueChanged(readingValue.Key);
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
