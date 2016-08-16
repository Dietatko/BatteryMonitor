using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public abstract class Pack : BatteryElement
	{
		protected Pack(IEnumerable<BatteryElement> subElements)
		{
			Contract.Requires(subElements, "subElements")
				.NotToBeNull();
			this.m_subElements = subElements.ToList();
			Contract.Requires(this.m_subElements, "subElements")
				.NotToBeEmpty();

			this.SubElements.ForEach(x => x.ValueChanged += (s, a) => this.OnValueChanged(a));
		}

		public IEnumerable<BatteryElement> SubElements
		{
			get { return this.m_subElements; }
		}
		private readonly List<BatteryElement> m_subElements; 

		public BatteryElement this[int index]
		{
			get { return this.m_subElements[index]; }
		}

		public int ElementCount
		{
			get { return this.m_subElements.Count; }
		}


		#region Reading values helpers

		protected IReadingValue CreateFallbackReadingValue<TValue>(IReadingValue computedReadingValue)
		{
			Contract.Requires(computedReadingValue, "computedReadingValue").NotToBeNull();

			return new FallbackReadingValue(
				computedReadingValue.Key,
				new TypedReadingValue<TValue>(computedReadingValue.Key),
				computedReadingValue);
		}

		protected IReadingValue CreateSameReadingValue<TValue>(EntryKey key, EntryKey targetKey)
		{
			return new MathFunctionReadingValue<TValue>(
				key,
				this.SubElements,
				targetKey,
				x => x.Distinct().Single(),
				(el, x) => x);
		}

		protected IReadingValue CreateSumReadingValue(EntryKey key, EntryKey targetKey)
		{
			return new MathFunctionReadingValue<float>(
				key,
				this.SubElements,
				targetKey,
				x => x.Sum(),
				(el, x) => x / el.Length);
		}

		protected IReadingValue CreateAverageReadingValue(EntryKey key, EntryKey targetKey)
		{
			return new MathFunctionReadingValue<float>(
				key,
				this.SubElements,
				targetKey,
				vals => vals.Average(x => x),
				(el, x) => x);
		}

		protected IReadingValue CreateMinReadingValue<TValue>(EntryKey key, EntryKey targetKey)
		{
			return new MathFunctionReadingValue<TValue>(
				key,
				this.SubElements,
				targetKey,
				x => x.Min(),
				(el, x) => x);
		}

		protected IReadingValue CreateMaxReadingValue<TValue>(EntryKey key, EntryKey targetKey)
		{
			return new MathFunctionReadingValue<TValue>(
				key,
				this.SubElements,
				targetKey,
				x => x.Max(),
				(el, x) => x);
		}

		#endregion Reading values helpers
	}
}
