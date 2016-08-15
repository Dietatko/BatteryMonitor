using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public partial class ParallelBatteryPack : BatteryPack
	{
		public ParallelBatteryPack(IEnumerable<BatteryElement> subElements)
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
					this.CreateSameReadingValue<float>(BatteryActualsWrapper.VoltageKey)));

			this.CustomData.CreateValue(
				DesignParametersWrapper.DesignedDischargeCurrentKey,
				this.CreateFallbackReadingValue<float>(
					new MathFunctionReadingValue<float>(
						this.SubElements,
						DesignParametersWrapper.DesignedDischargeCurrentKey,
						x =>
						{
							var valList = x.ToList();
							return valList.Min() * valList.Count;
						})));

			this.CustomData.CreateValue(
				DesignParametersWrapper.MaxDischargeCurrentKey,
				this.CreateFallbackReadingValue<float>(
					new MathFunctionReadingValue<float>(
						this.SubElements,
						DesignParametersWrapper.MaxDischargeCurrentKey,
						x =>
						{
							var valList = x.ToList();
							return valList.Min() * valList.Count;
						})));

			this.CustomData.CreateValue(
				DesignParametersWrapper.DesignedCapacityKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateSumReadingValue(DesignParametersWrapper.DesignedCapacityKey)));
		}

		private void CreateHealthReadings()
		{
			this.CustomData.CreateValue(
				BatteryHealthWrapper.FullChargeCapacityKey,
				this.CreateFallbackReadingValue<float>(
					new MathFunctionReadingValue<float>(
						this.SubElements,
						BatteryHealthWrapper.FullChargeCapacityKey,
						x =>
						{
							var valList = x.ToList();
							return valList.Min() * valList.Count;
						})));
		}

		private void CreateActualReadings()
		{
			this.CustomData.CreateValue(
				BatteryActualsWrapper.VoltageKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateSameReadingValue<float>(BatteryActualsWrapper.VoltageKey)));

			this.CustomData.CreateValue(
				BatteryActualsWrapper.ActualCurrentKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateSumReadingValue(BatteryActualsWrapper.ActualCurrentKey)));

			this.CustomData.CreateValue(
				BatteryActualsWrapper.AverageCurrentKey,
				this.CreateFallbackReadingValue<float>(
					this.CreateSumReadingValue(BatteryActualsWrapper.AverageCurrentKey)));

			this.CustomData.CreateValue(
				BatteryActualsWrapper.RemainingCapacityKey,
				this.CreateFallbackReadingValue<float>(
					new MathFunctionReadingValue<float>(
						this.SubElements,
						BatteryActualsWrapper.RemainingCapacityKey,
						x =>
						{
							var valList = x.ToList();
							return valList.Min() * valList.Count;
						},
						(el, x) => x / el.Length)));
		}
	}
}
