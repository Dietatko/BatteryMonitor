using System;
using System.Collections.Generic;
using System.Linq;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public class FallbackReadingValue : ReadingValueBase, IReadingValue
	{
		public FallbackReadingValue(EntryKey key, params IReadingValue[] subValues)
			: this(key, (IEnumerable<IReadingValue>)subValues)
		{
		}

		public FallbackReadingValue(EntryKey key, IEnumerable<IReadingValue> subValues)
			: base(key)
		{
			Contract.Requires(subValues, "subValues").NotToBeNull();
			this.SubValues = subValues.ToList();
			Contract.Requires(this.SubValues, "subValues").NotToBeEmpty();

			this.SubValues.ForEach(x => x.ValueChanged += this.OnReadingValueChanged );
		}

		public IEnumerable<IReadingValue> SubValues { get; private set; }

		public bool IsDefined
		{
			get { return this.SubValues.Any(x => x.IsDefined); }
		}

		public T Get<T>()
		{
			var definedValue = this.SubValues.FirstOrDefault(x => x.IsDefined);
			return (definedValue != null ? definedValue.Get<T>() : default(T));
		}

		public void Set(object value)
		{
			var firstValue = this.SubValues.FirstOrDefault();
			if (firstValue == null)
				throw new InvalidOperationException("The reading vlaue is read-only");

			firstValue.Set(value);
		}

		public void Reset()
		{
			this.SubValues.ForEach(x => x.Reset());
		}

		private void OnReadingValueChanged(object sender, EventArgs eventArgs)
		{
			var changedReadingValue = (IReadingValue)sender;

			foreach (var value in this.SubValues)
			{
				if (ReferenceEquals(value, changedReadingValue))
				{
					this.OnValueChanged();
					break;
				}

				if (value.IsDefined)
					break;
			}
		}
	}
}
