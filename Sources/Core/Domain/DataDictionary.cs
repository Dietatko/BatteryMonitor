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
			this.Storage = new ConcurrentDictionary<EntryIdentifier, object>();
		}

		protected ConcurrentDictionary<EntryIdentifier, object> Storage { get; private set; }

		public bool HasValue(string namespaceUri, string entryName)
		{
			var entryId = new EntryIdentifier(namespaceUri, entryName);
			return this.Storage.ContainsKey(entryId);
		}

		public IEnumerable<EntryIdentifier> GetValues(string namespaceUri = null)
		{
			IEnumerable<EntryIdentifier> entryIds = this.Storage.Keys;
			if (namespaceUri != null)
				entryIds = entryIds.Where(x => x.NamespaceUri == namespaceUri);

			return entryIds;
		}

		public T GetValue<T>(string namespaceUri, string entryName)
		{
			var entryId = new EntryIdentifier(namespaceUri, entryName);
			var entryValue = this.Storage[entryId];

			return (T)Convert.ChangeType(entryValue, typeof(T));
		}

		public bool TryGetValue<T>(string namespaceUri, string entryName, out T value)
		{
			var entryId = new EntryIdentifier(namespaceUri, entryName);

			object tmpValue;
			if (!this.Storage.TryGetValue(entryId, out tmpValue))
			{
				value = default(T);
				return false;
			}

			value = (T)Convert.ChangeType(tmpValue, typeof(T));
			return true;
		}

		public void SetValue<T>(string namespaceUri, string entryName, T value)
		{
			var entryId = new EntryIdentifier(namespaceUri, entryName);
			this.Storage[entryId] = value;
		}

		public void Merge(DataDictionary source)
		{
			Contract.Requires(source, "source").IsNotNull();

			foreach (var entryId in source.GetValues())
			{
				this.SetValue(entryId.NamespaceUri, entryId.EntryName, source.GetValue<object>(entryId.NamespaceUri, entryId.EntryName));
			}
		}
	}
}
