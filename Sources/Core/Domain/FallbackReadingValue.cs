using System;
using System.Collections.Generic;
using System.Linq;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public class FallbackReadingValue : IReadingValue
	{
		public FallbackReadingValue(params IReadingValue[] subValues)
			: this((IEnumerable<IReadingValue>)subValues)
		{
		}

		public FallbackReadingValue(IEnumerable<IReadingValue> subValues)
		{
			Contract.Requires(subValues, "subValues").NotToBeNull();
			this.SubValues = subValues.ToList();
			Contract.Requires(this.SubValues, "subValues").NotToBeEmpty();
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
	}
}
