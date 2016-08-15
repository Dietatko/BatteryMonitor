using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public class SingleCell : BatteryElement
	{
		public SingleCell(float nominalVoltage, float designedDischargeCurrent, float maxDischargeCurrent, float designedCapacity)
		{
			this.InitializeReadings();

			var designParameters = this.DesignParameters();
			designParameters.NominalVoltage = nominalVoltage;
			designParameters.DesignedDischargeCurrent = designedDischargeCurrent;
			designParameters.MaxDischargeCurrent = maxDischargeCurrent;
			designParameters.DesignedCapacity = designedCapacity;
		}

		protected void InitializeReadings()
		{
			this.CreateDesignParametersReadings();
			this.CreateHealthReadings();
			this.CreateActualReadings();
		}

		private void CreateDesignParametersReadings()
		{
			this.CustomData.CreateValue(DesignParametersWrapper.NominalVoltageKey, new TypedReadingValue<float>());
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
			this.CustomData.CreateValue(BatteryActualsWrapper.VoltageKey, new TypedReadingValue<float>());
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
