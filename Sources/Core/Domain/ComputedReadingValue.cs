using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

using ImpruvIT.BatteryMonitor.Domain.Battery;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public class ComputedReadingValue : IReadingValue
	{
		public ComputedReadingValue(
			IEnumerable<BatteryElement> elements,
			Func<BatteryElement[], bool> isDefinedFunction,
			Func<BatteryElement[], object> getValueFunction,
			Action<BatteryElement[], object> setValueFunction = null,
			Action<BatteryElement[]> resetFunction = null)
		{
			Contract.Requires(elements, "elements").NotToBeNull();
			Contract.Requires(isDefinedFunction, "isDefinedFunction").NotToBeNull();
			Contract.Requires(getValueFunction, "getValueFunction").NotToBeNull();

			this.Elements = elements.ToArray();
			this.IsDefinedFunction = isDefinedFunction;
			this.GetValueFunction = getValueFunction;
			this.SetValueFunction = setValueFunction;
			this.ResetFunction = resetFunction;
		}

		protected BatteryElement[] Elements { get; private set; }

		protected Func<BatteryElement[], bool> IsDefinedFunction { get; set; }

		protected Func<BatteryElement[], object> GetValueFunction { get; set; }

		protected Action<BatteryElement[], object> SetValueFunction { get; set; }

		protected Action<BatteryElement[]> ResetFunction { get; set; }

		public bool IsDefined
		{
			get { return this.IsDefinedFunction(this.Elements); }
		}

		public T Get<T>()
		{
			var tmpValue = this.GetValueFunction(this.Elements);
			return (T)Convert.ChangeType(tmpValue, typeof(T));
		}

		public void Set(object value)
		{
			if (this.SetValueFunction == null)
				throw new InvalidOperationException("The reading value is read-only.");

			this.SetValueFunction(this.Elements, value);
		}

		public void Reset()
		{
			if (this.ResetFunction == null)
				throw new InvalidOperationException("The reading value is read-only.");

			this.ResetFunction(this.Elements);
		}
	}
}
