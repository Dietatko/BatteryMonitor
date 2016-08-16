using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Domain.Battery;

namespace ImpruvIT.BatteryMonitor.Protocols.SMBus
{
	public class BatteryPack : SeriesPack
	{
		public BatteryPack(IEnumerable<BatteryElement> subElements)
			: base(subElements)
		{
			this.InitializeReadings();
		}

		protected void InitializeReadings()
		{
			this.CreateProductDefinitionReadings();
			this.CreateDesignParametersReadings();
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

		private void CreateDesignParametersReadings()
		{
			this.CustomData.CreateValue(
				DesignParametersWrapper.NominalVoltageKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateSumReadingValue(BatteryActualsWrapper.VoltageKey)));

			this.CustomData.CreateValue(DesignParametersWrapper.DesignedDischargeCurrentKey, new TypedReadingValue<float>());
			this.CustomData.CreateValue(DesignParametersWrapper.MaxDischargeCurrentKey, new TypedReadingValue<float>());
			this.CustomData.CreateValue(DesignParametersWrapper.DesignedCapacityKey, new TypedReadingValue<float>());
		}

		private void CreateHealthReadings()
		{
			this.CustomData.CreateValue(BatteryHealthWrapper.FullChargeCapacityKey, new TypedReadingValue<float>());
			this.CustomData.CreateValue(BatteryHealthWrapper.CycleCountKey, new TypedReadingValue<int>());
			this.CustomData.CreateValue(BatteryHealthWrapper.CalculationPrecisionKey, new TypedReadingValue<float>());
		}

		private void CreateActualReadings()
		{
			this.CustomData.CreateValue(
				BatteryActualsWrapper.VoltageKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateSumReadingValue(BatteryActualsWrapper.VoltageKey)));

			this.CustomData.CreateValue(BatteryActualsWrapper.ActualCurrentKey, new TypedReadingValue<float>());
			this.CustomData.CreateValue(BatteryActualsWrapper.AverageCurrentKey, new TypedReadingValue<float>());
			this.CustomData.CreateValue(BatteryActualsWrapper.TemperatureKey, new TypedReadingValue<float>());

			this.CustomData.CreateValue(BatteryActualsWrapper.RemainingCapacityKey, new TypedReadingValue<float>());
			this.CustomData.CreateValue(BatteryActualsWrapper.AbsoluteStateOfChargeKey, new TypedReadingValue<float>());
			this.CustomData.CreateValue(BatteryActualsWrapper.RelativeStateOfChargeKey, new TypedReadingValue<float>());

			this.CustomData.CreateValue(BatteryActualsWrapper.ActualRunTimeKey, new TypedReadingValue<TimeSpan>());
			this.CustomData.CreateValue(BatteryActualsWrapper.AverageRunTimeKey, new TypedReadingValue<TimeSpan>());
		}
	}
}
