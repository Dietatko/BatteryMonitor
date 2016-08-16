using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain.Battery;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public class MathFunctionReadingValue<TValue> : ReadingValueBase, IReadingValue
	{
		public MathFunctionReadingValue(
			EntryKey key,
			IEnumerable<BatteryElement> elements,
			EntryKey targetKey,
			Func<IEnumerable<TValue>, TValue> getSelector)
			: this(key, elements, targetKey, getSelector, null)
		{
		}

		public MathFunctionReadingValue(
			EntryKey key,
			IEnumerable<BatteryElement> elements,
			EntryKey targetKey,
			Func<IEnumerable<TValue>, TValue> getSelector,
			Func<BatteryElement[], TValue, TValue> setSelector)
			: base(key)
		{
			Contract.Requires(elements, "elements").NotToBeNull();
			Contract.Requires(targetKey, "targetKey").NotToBeNull();
			Contract.Requires(getSelector, "getSelector").NotToBeNull();

			this.Elements = elements.ToArray();
			this.Elements.ForEach(x => x.ValueChanged += this.OnReadingValueChanged);
			this.TargetKey = targetKey;
			this.GetSelector = getSelector;
			this.SetSelector = setSelector;
		}

		protected BatteryElement[] Elements { get; private set; }

		protected EntryKey TargetKey { get; private set; }

		public Func<IEnumerable<TValue>, TValue> GetSelector { get; private set; }

		public Func<BatteryElement[], TValue, TValue> SetSelector { get; private set; }

		public bool IsDefined
		{
			get { return this.Elements.All(x => x.CustomData.GetValue(this.TargetKey).IsDefined); }
		}

		public T Get<T>()
		{
			var tmpValue = this.GetSelector(this.Elements.Select(y => y.CustomData.GetValue(this.TargetKey).Get<TValue>()));
			return (T)Convert.ChangeType(tmpValue, typeof(T));
		}

		public void Set(object value)
		{
			if (this.SetSelector == null)
				throw new InvalidOperationException("The reading value is read-only.");

			var elementValue = this.SetSelector(this.Elements, (TValue)value);
			this.Elements.ForEach(x => x.CustomData.GetValue(this.TargetKey).Set(elementValue));
		}

		public void Reset()
		{
			this.Elements.ForEach(x => x.CustomData.GetValue(this.TargetKey).Reset());
		}

		private void OnReadingValueChanged(object sender, EntryKey entryKey)
		{
			if (this.TargetKey.Equals(entryKey))
				this.OnValueChanged();
		}
	}
}
