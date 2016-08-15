using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public partial class SeriesBatteryPack : BatteryPack
	{
		private readonly SeriesPackDesignParameters m_params;
		private readonly SeriesPackHealth m_health;
		private readonly SeriesPackActuals m_actuals;

		public SeriesBatteryPack(IEnumerable<BatteryElement> subElements)
			: base(subElements)
		{
			this.m_params = new SeriesPackDesignParameters(this.SubElements);
			this.m_health = new SeriesPackHealth(this.SubElements);
			this.m_actuals = new SeriesPackActuals(this.SubElements);
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
