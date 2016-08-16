using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public class ParallelPack : Pack
	{
		public ParallelPack(IEnumerable<BatteryElement> subElements)
			: base(subElements)
		{
			this.InitializeReadings();
		}

		protected void InitializeReadings()
		{
			this.CreateDesignParametersReadings();
			this.CreateHealthReadings();
			this.CreateActualReadings();
		}

		private void CreateDesignParametersReadings()
		{
			this.CustomData.CreateValue(
				this.CreateFallbackReadingValue<float>(
					this.CreateSameReadingValue<float>(DesignParametersWrapper.NominalVoltageKey, BatteryActualsWrapper.VoltageKey)));

			this.CustomData.CreateValue(
				this.CreateFallbackReadingValue<float>(
					new MathFunctionReadingValue<float>(
						DesignParametersWrapper.DesignedDischargeCurrentKey,
						this.SubElements,
						DesignParametersWrapper.DesignedDischargeCurrentKey,
						x =>
						{
							var valList = x.ToList();
							return valList.Min() * valList.Count;
						})));

			this.CustomData.CreateValue(
				this.CreateFallbackReadingValue<float>(
					new MathFunctionReadingValue<float>(
						DesignParametersWrapper.MaxDischargeCurrentKey,
						this.SubElements,
						DesignParametersWrapper.MaxDischargeCurrentKey,
						x =>
						{
							var valList = x.ToList();
							return valList.Min() * valList.Count;
						})));

			this.CustomData.CreateValue(
				this.CreateFallbackReadingValue<float>(
					this.CreateSumReadingValue(DesignParametersWrapper.DesignedCapacityKey, DesignParametersWrapper.DesignedCapacityKey)));
		}

		private void CreateHealthReadings()
		{
			this.CustomData.CreateValue(
				this.CreateFallbackReadingValue<float>(
					new MathFunctionReadingValue<float>(
						BatteryHealthWrapper.FullChargeCapacityKey,
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
				this.CreateFallbackReadingValue<float>(
					this.CreateSameReadingValue<float>(BatteryActualsWrapper.VoltageKey, BatteryActualsWrapper.VoltageKey)));

			this.CustomData.CreateValue(
				this.CreateFallbackReadingValue<float>(
					this.CreateSumReadingValue(BatteryActualsWrapper.ActualCurrentKey, BatteryActualsWrapper.ActualCurrentKey)));

			this.CustomData.CreateValue(
				this.CreateFallbackReadingValue<float>(
					this.CreateSumReadingValue(BatteryActualsWrapper.AverageCurrentKey, BatteryActualsWrapper.AverageCurrentKey)));

			this.CustomData.CreateValue(
				this.CreateFallbackReadingValue<float>(
					new MathFunctionReadingValue<float>(
						BatteryActualsWrapper.RemainingCapacityKey,
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
