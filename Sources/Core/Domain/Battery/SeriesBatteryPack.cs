using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public class SeriesBatteryPack : BatteryPack
	{
		public SeriesBatteryPack(IEnumerable<BatteryElement> subElements)
			: base(subElements)
		{
			this.InitializeReadings();
		}

		protected new void InitializeReadings()
		{
			base.InitializeReadings();

			this.CreateDesignParametersReadings();
			this.CreateHealthReadings();
			this.CreateActualReadings();
		}

		private void CreateDesignParametersReadings()
		{
			this.CustomData.CreateValue(
				DesignParametersWrapper.NominalVoltageKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateSumReadingValue(BatteryActualsWrapper.VoltageKey)));

			this.CustomData.CreateValue(
				DesignParametersWrapper.DesignedDischargeCurrentKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateMinReadingValue<float>(DesignParametersWrapper.DesignedDischargeCurrentKey)));

			this.CustomData.CreateValue(
				DesignParametersWrapper.MaxDischargeCurrentKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateMinReadingValue<float>(DesignParametersWrapper.MaxDischargeCurrentKey)));

			this.CustomData.CreateValue(
				DesignParametersWrapper.DesignedCapacityKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateMinReadingValue<float>(DesignParametersWrapper.DesignedCapacityKey)));
		}

		private void CreateHealthReadings()
		{
			this.CustomData.CreateValue(
				BatteryHealthWrapper.FullChargeCapacityKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateMinReadingValue<float>(BatteryHealthWrapper.FullChargeCapacityKey)));
		}

		private void CreateActualReadings()
		{
			this.CustomData.CreateValue(
				BatteryActualsWrapper.VoltageKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateSumReadingValue(BatteryActualsWrapper.VoltageKey)));

			this.CustomData.CreateValue(
			    BatteryActualsWrapper.ActualCurrentKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateSameReadingValue<float>(BatteryActualsWrapper.ActualCurrentKey)));

			this.CustomData.CreateValue(
				BatteryActualsWrapper.AverageCurrentKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateSameReadingValue<float>(BatteryActualsWrapper.AverageCurrentKey)));

			this.CustomData.CreateValue(
				BatteryActualsWrapper.RemainingCapacityKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateMinReadingValue<float>(BatteryActualsWrapper.RemainingCapacityKey)));
		}
	}
}
