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
			this.CreateSMBusReadings();
		}

		private void CreateProductDefinitionReadings()
		{
			this.CustomData.CreateValue(new TypedReadingValue<string>(ProductDefinitionWrapper.ManufacturerKey));
			this.CustomData.CreateValue(new TypedReadingValue<string>(ProductDefinitionWrapper.ProductKey));
			this.CustomData.CreateValue(new TypedReadingValue<string>(ProductDefinitionWrapper.ChemistryKey));
			this.CustomData.CreateValue(new TypedReadingValue<DateTime>(ProductDefinitionWrapper.ManufactureDateKey));
			this.CustomData.CreateValue(new TypedReadingValue<string>(ProductDefinitionWrapper.SerialNumberKey));
		}

		private void CreateDesignParametersReadings()
		{
			this.CustomData.CreateValue(
				this.CreateFallbackReadingValue<float>(
					this.CreateSumReadingValue(DesignParametersWrapper.NominalVoltageKey, DesignParametersWrapper.NominalVoltageKey)));

			this.CustomData.CreateValue(new TypedReadingValue<float>(DesignParametersWrapper.DesignedDischargeCurrentKey));
			this.CustomData.CreateValue(new TypedReadingValue<float>(DesignParametersWrapper.MaxDischargeCurrentKey));
			this.CustomData.CreateValue(new TypedReadingValue<float>(DesignParametersWrapper.DesignedCapacityKey));
		}

		private void CreateHealthReadings()
		{
			this.CustomData.CreateValue(new TypedReadingValue<float>(BatteryHealthWrapper.FullChargeCapacityKey));
			this.CustomData.CreateValue(new TypedReadingValue<int>(BatteryHealthWrapper.CycleCountKey));
			this.CustomData.CreateValue(new TypedReadingValue<float>(BatteryHealthWrapper.CalculationPrecisionKey));
		}

		private void CreateActualReadings()
        {
            this.CustomData.CreateValue(new TypedReadingValue<ushort>(BatteryActualsWrapper.BatteryStatusKey));

            this.CustomData.CreateValue(
				this.CreateFallbackReadingValue<float>(
					this.CreateSumReadingValue(BatteryActualsWrapper.VoltageKey, BatteryActualsWrapper.VoltageKey)));

			this.CustomData.CreateValue(new TypedReadingValue<float>(BatteryActualsWrapper.ActualCurrentKey));
			this.CustomData.CreateValue(new TypedReadingValue<float>(BatteryActualsWrapper.AverageCurrentKey));
			this.CustomData.CreateValue(new TypedReadingValue<float>(BatteryActualsWrapper.TemperatureKey));

            this.CustomData.CreateValue(new TypedReadingValue<float>(BatteryActualsWrapper.ChargingVoltageKey));
            this.CustomData.CreateValue(new TypedReadingValue<float>(BatteryActualsWrapper.ChargingCurrentKey));

            this.CustomData.CreateValue(new TypedReadingValue<float>(BatteryActualsWrapper.RemainingCapacityKey));
			this.CustomData.CreateValue(new TypedReadingValue<float>(BatteryActualsWrapper.AbsoluteStateOfChargeKey));
			this.CustomData.CreateValue(new TypedReadingValue<float>(BatteryActualsWrapper.RelativeStateOfChargeKey));

			this.CustomData.CreateValue(new TypedReadingValue<TimeSpan>(BatteryActualsWrapper.ActualRunTimeKey));
			this.CustomData.CreateValue(new TypedReadingValue<TimeSpan>(BatteryActualsWrapper.AverageRunTimeKey));
		}

		private void CreateSMBusReadings()
		{
			this.CustomData.CreateValue(new TypedReadingValue<ushort>(SMBusDataWrapper.BatteryModeKey));
			this.CustomData.CreateValue(new TypedReadingValue<int>(SMBusDataWrapper.CellCountKey));
			this.CustomData.CreateValue(new TypedReadingValue<Version>(SMBusDataWrapper.SpecificationVersionKey));
			this.CustomData.CreateValue(new TypedReadingValue<int>(SMBusDataWrapper.VoltageScaleKey));
			this.CustomData.CreateValue(new TypedReadingValue<int>(SMBusDataWrapper.CurrentScaleKey));
		}
	}
}
