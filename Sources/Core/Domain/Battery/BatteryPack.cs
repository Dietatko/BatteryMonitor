using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public abstract class BatteryPack : BatteryElement
	{
		protected BatteryPack(IEnumerable<BatteryElement> subElements)
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


		protected void InitializeReadings()
		{
			this.CreateProductDefinitionReadings();
			this.CreateHealthReadings();
			this.CreateActualReadings();
		}

		private void CreateProductDefinitionReadings()
		{
			this.CustomData.CreateValue(ProductDefinitionWrapper.ManufacturerKey, new TypedReadingValue<string>());
			this.CustomData.CreateValue(ProductDefinitionWrapper.ProductKey, new TypedReadingValue<string>());
			this.CustomData.CreateValue(ProductDefinitionWrapper.ChemistryKey, new TypedReadingValue<string>());
			this.CustomData.CreateValue(ProductDefinitionWrapper.ManufactureDateKey, new TypedReadingValue<DateTime>());
			this.CustomData.CreateValue(ProductDefinitionWrapper.SerialNumberKey, new TypedReadingValue<string>());
		}

		private void CreateHealthReadings()
		{
			this.CustomData.CreateValue(
				BatteryHealthWrapper.CycleCountKey,
				this.CreateFallbackReadingValue<int>(
					this.CreateMaxReadingValue<int>(BatteryHealthWrapper.CycleCountKey)));

			this.CustomData.CreateValue(
				BatteryHealthWrapper.CalculationPrecisionKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateMaxReadingValue<float>(BatteryHealthWrapper.CalculationPrecisionKey)));
		}

		private void CreateActualReadings()
		{
			this.CustomData.CreateValue(
				BatteryActualsWrapper.TemperatureKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateMaxReadingValue<float>(BatteryActualsWrapper.TemperatureKey)));

			this.CustomData.CreateValue(
				BatteryActualsWrapper.AbsoluteStateOfChargeKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateAverageReadingValue(BatteryActualsWrapper.AbsoluteStateOfChargeKey)));

			this.CustomData.CreateValue(
				BatteryActualsWrapper.RelativeStateOfChargeKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateAverageReadingValue(BatteryActualsWrapper.RelativeStateOfChargeKey)));

			this.CustomData.CreateValue(
				BatteryActualsWrapper.ActualRunTimeKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateMinReadingValue<float>(BatteryActualsWrapper.ActualRunTimeKey)));

			this.CustomData.CreateValue(
				BatteryActualsWrapper.AverageRunTimeKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateMinReadingValue<float>(BatteryActualsWrapper.AverageRunTimeKey)));
		}


		#region Reading values helpers

		protected IReadingValue CreateFallbackReadingValue<TValue>(IReadingValue computedReadingValue)
		{
			Contract.Requires(computedReadingValue, "computedReadingValue").NotToBeNull();

			return new FallbackReadingValue(
				new TypedReadingValue<TValue>(),
				computedReadingValue);
		}

		protected IReadingValue CreateSameReadingValue<TValue>(EntryKey key)
		{
			return new MathFunctionReadingValue<TValue>(
				this.SubElements,
				key,
				x => x.Distinct().Single(),
				(el, x) => x);
		}

		protected IReadingValue CreateSumReadingValue(EntryKey key)
		{
			return new MathFunctionReadingValue<float>(
				this.SubElements,
				key,
				x => x.Sum(),
				(el, x) => x / el.Length);
		}

		protected IReadingValue CreateAverageReadingValue(EntryKey key)
		{
			return new MathFunctionReadingValue<float>(
				this.SubElements,
				key,
				vals => vals.Average(x => x),
				(el, x) => x);
		}

		protected IReadingValue CreateMinReadingValue<TValue>(EntryKey key)
		{
			return new MathFunctionReadingValue<TValue>(
				this.SubElements,
				key,
				x => x.Min(),
				(el, x) => x);
		}

		protected IReadingValue CreateMaxReadingValue<TValue>(EntryKey key)
		{
			return new MathFunctionReadingValue<TValue>(
				this.SubElements,
				key,
				x => x.Max(),
				(el, x) => x);
		}

		#endregion Reading values helpers
	}
}
