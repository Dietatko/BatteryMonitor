using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public partial class ParallelBatteryPack : BatteryPack
	{
		private readonly ParallelPackDesignParameters m_params;
		private readonly ParallelPackHealth m_health;
		private readonly ParallelPackActuals m_actuals;

		public ParallelBatteryPack(IEnumerable<BatteryElement> subElements)
			: base(subElements)
		{
			this.m_params = new ParallelPackDesignParameters(this.SubElements);
			this.m_health = new ParallelPackHealth(this.SubElements);
			this.m_actuals = new ParallelPackActuals(this.SubElements);
		}

		public override IDesignParameters DesignParameters
		{
			get { return this.m_params; }
		}

		public override IBatteryHealth Health
		{
			get { return this.m_health; }
		}

		public override IBatteryActuals Actuals
		{
			get { return this.m_actuals; }
		}

		protected override void InitializeCustomData()
		{
			base.InitializeCustomData();

			this.CreateActualReadings();
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
