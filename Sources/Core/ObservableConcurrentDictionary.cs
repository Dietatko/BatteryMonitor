using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ImpruvIT.BatteryMonitor
{
	/// <summary>
	/// Provides a thread-safe dictionary for use with data binding.
	/// </summary>
	/// <typeparam name="TKey">Specifies the type of the keys in this collection.</typeparam>
	/// <typeparam name="TValue">Specifies the type of the values in this collection.</typeparam>
	[DebuggerDisplay("Count={Count}")]
	public class ObservableConcurrentDictionary<TKey, TValue> : IDictionary<TKey, TValue>,
		INotifyCollectionChanged, INotifyPropertyChanged
	{
		private readonly SynchronizationContext m_context;
		private readonly ConcurrentDictionary<TKey, TValue> m_dictionary;

		/// <summary>
		/// Initializes an instance of the ObservableConcurrentDictionary class.
		/// </summary>
		public ObservableConcurrentDictionary()
		{
			this.m_context = AsyncOperationManager.SynchronizationContext;
			this.m_dictionary = new ConcurrentDictionary<TKey, TValue>();
		}

		/// <summary>Event raised when the collection changes.</summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;
		/// <summary>Event raised when a property on the collection changes.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Notifies observers of CollectionChanged or PropertyChanged of an update to the dictionary.
		/// </summary>
		private void NotifyObserversOfChange()
		{
			var collectionHandler = CollectionChanged;
			var propertyHandler = PropertyChanged;
			if (collectionHandler != null || propertyHandler != null)
			{
				this.m_context.Post(s =>
				{
					if (collectionHandler != null)
					{
						collectionHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
					}
					if (propertyHandler != null)
					{
						propertyHandler(this, new PropertyChangedEventArgs("Count"));
						propertyHandler(this, new PropertyChangedEventArgs("Keys"));
						propertyHandler(this, new PropertyChangedEventArgs("Values"));
					}
				}, null);
			}
		}

		/// <summary>Attempts to add an item to the dictionary, notifying observers of any changes.</summary>
		/// <param name="item">The item to be added.</param>
		/// <returns>Whether the add was successful.</returns>
		private bool TryAddWithNotification(KeyValuePair<TKey, TValue> item)
		{
			return TryAddWithNotification(item.Key, item.Value);
		}

		/// <summary>Attempts to add an item to the dictionary, notifying observers of any changes.</summary>
		/// <param name="key">The key of the item to be added.</param>
		/// <param name="value">The value of the item to be added.</param>
		/// <returns>Whether the add was successful.</returns>
		private bool TryAddWithNotification(TKey key, TValue value)
		{
			bool result = this.m_dictionary.TryAdd(key, value);
			if (result) NotifyObserversOfChange();
			return result;
		}

		/// <summary>Attempts to remove an item from the dictionary, notifying observers of any changes.</summary>
		/// <param name="key">The key of the item to be removed.</param>
		/// <param name="value">The value of the item removed.</param>
		/// <returns>Whether the removal was successful.</returns>
		private bool TryRemoveWithNotification(TKey key, out TValue value)
		{
			bool result = this.m_dictionary.TryRemove(key, out value);
			if (result) NotifyObserversOfChange();
			return result;
		}

		/// <summary>Attempts to add or update an item in the dictionary, notifying observers of any changes.</summary>
		/// <param name="key">The key of the item to be updated.</param>
		/// <param name="value">The new value to set for the item.</param>
		/// <returns>Whether the update was successful.</returns>
		private void UpdateWithNotification(TKey key, TValue value)
		{
			this.m_dictionary[key] = value;
			NotifyObserversOfChange();
		}

		#region ICollection<KeyValuePair<TKey,TValue>> Members

		void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
		{
			TryAddWithNotification(item);
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
		{
			return ((ICollection<KeyValuePair<TKey, TValue>>)this.m_dictionary).Contains(item);
		}

		void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			((ICollection<KeyValuePair<TKey, TValue>>)this.m_dictionary).CopyTo(array, arrayIndex);
		}

		int ICollection<KeyValuePair<TKey, TValue>>.Count
		{
			get { return ((ICollection<KeyValuePair<TKey, TValue>>)this.m_dictionary).Count; }
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
		{
			get { return ((ICollection<KeyValuePair<TKey, TValue>>)this.m_dictionary).IsReadOnly; }
		}

		bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
		{
			TValue temp;
			return TryRemoveWithNotification(item.Key, out temp);
		}

		#endregion

		public void Add(TKey key, TValue value)
		{
			TryAddWithNotification(key, value);
		}

		public void Clear()
		{
			((ICollection<KeyValuePair<TKey, TValue>>)this.m_dictionary).Clear();
			NotifyObserversOfChange();
		}

		public bool ContainsKey(TKey key)
		{
			return this.m_dictionary.ContainsKey(key);
		}

		public ICollection<TKey> Keys
		{
			get { return this.m_dictionary.Keys; }
		}

		public bool Remove(TKey key)
		{
			TValue temp;
			return TryRemoveWithNotification(key, out temp);
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			return this.m_dictionary.TryGetValue(key, out value);
		}

		public ICollection<TValue> Values
		{
			get { return this.m_dictionary.Values; }
		}

		public TValue this[TKey key]
		{
			get { return this.m_dictionary[key]; }
			set { UpdateWithNotification(key, value); }
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			return this.m_dictionary.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}
