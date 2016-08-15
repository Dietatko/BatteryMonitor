using System;
using System.Collections.Generic;
using System.Linq;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public class SingleCell : BatteryElement
	{
		private readonly ProductDefinitionWrapper m_productWrapper;
		private readonly DesignParametersWrapper m_paramsWrapper;
		private readonly BatteryHealthWrapper m_healthWrapper;
		private readonly BatteryActualsWrapper m_actualsWrapper;

		public SingleCell(float nominalVoltage, float designedDischargeCurrent, float maxDischargeCurrent, float designedCapacity)
		{
			Contract.Requires(nominalVoltage, "nominalVoltage").ToBeInRange(x => x >= 0);
			Contract.Requires(designedDischargeCurrent, "designedDischargeCurrent").ToBeInRange(x => x >= 0);
			Contract.Requires(maxDischargeCurrent, "maxDischargeCurrent").ToBeInRange(x => x >= 0);
			Contract.Requires(designedCapacity, "designedCapacity").ToBeInRange(x => x >= 0);

			this.m_productWrapper = new ProductDefinitionWrapper(this.CustomData);

			this.m_paramsWrapper = new DesignParametersWrapper(this.CustomData);
			this.m_paramsWrapper.NominalVoltage = nominalVoltage;
			this.m_paramsWrapper.DesignedDischargeCurrent = designedDischargeCurrent;
			this.m_paramsWrapper.MaxDischargeCurrent = maxDischargeCurrent;
			this.m_paramsWrapper.DesignedCapacity = designedCapacity;

			this.m_healthWrapper = new BatteryHealthWrapper(this.CustomData);
			this.m_actualsWrapper = new BatteryActualsWrapper(this.CustomData);
		}

		public override IProductDefinition Product
		{
			get { return this.m_productWrapper; }
		}

		public override IDesignParameters DesignParameters
		{
			get { return this.m_paramsWrapper; }
		}


		#region Battery health

		public override IBatteryHealth Health
		{
			get { return this.m_healthWrapper; }
		}

		public void SetFullChargeCapacity(float fullChargeCapacity)
		{
			Contract.Requires(fullChargeCapacity, "fullChargeCapacity").ToBeInRange(x => x >= 0);

			this.m_healthWrapper.FullChargeCapacity = fullChargeCapacity;
		}

		public void SetCycleCount(int cycleCount)
		{
			Contract.Requires(cycleCount, "cycleCount").ToBeInRange(x => x >= 0);

			this.m_healthWrapper.CycleCount = cycleCount;
		}

		public void SetCalculationPrecision(float calculationPrecision)
		{
			Contract.Requires(calculationPrecision, "calculationPrecision").ToBeInRange(x => 0f <= x && x <= 1f);

			this.m_healthWrapper.CalculationPrecision = calculationPrecision;
		}

		#endregion Battery health


		#region Actuals

		public override IBatteryActuals Actuals
		{
			get { return this.m_actualsWrapper; }
		}

		public void SetVoltage(float voltage)
		{
			Contract.Requires(voltage, "voltage").ToBeInRange(x => x >= 0f);

			this.m_actualsWrapper.Voltage = voltage;
		}

		public void SetActualCurrent(float actualCurrent)
		{
			this.m_actualsWrapper.ActualCurrent = actualCurrent;
		}

		public void SetAverageCurrent(float averageCurrent)
		{
			this.m_actualsWrapper.AverageCurrent = averageCurrent;
		}

		public void SetTemperature(float temperature)
		{
			this.m_actualsWrapper.Temperature = temperature;
		}

		public void SetRemainingCapacity(float remainingCapacity)
		{
			Contract.Requires(remainingCapacity, "remainingCapacity").ToBeInRange(x => x >= 0f);

			this.m_actualsWrapper.RemainingCapacity = remainingCapacity;
		}

		public void SetAbsoluteStateOfCharge(float absoluteStateOfCharge)
		{
			Contract.Requires(absoluteStateOfCharge, "cycleCount").ToBeInRange(x => x >= 0f);

			this.m_actualsWrapper.AbsoluteStateOfCharge = absoluteStateOfCharge;
		}

		public void SetRelativeStateOfCharge(float relativeStateOfCharge)
		{
			Contract.Requires(relativeStateOfCharge, "relativeStateOfCharge").ToBeInRange(x => 0f <= x && x <= 1f);

			this.m_actualsWrapper.RelativeStateOfCharge = relativeStateOfCharge;
		}

		public void SetActualRunTime(TimeSpan actualRunTime)
		{
			Contract.Requires(actualRunTime, "currentRunTime").ToBeInRange(x => x >= TimeSpan.Zero);

			this.m_actualsWrapper.ActualRunTime = actualRunTime;
		}

		public void SetAverageRunTime(TimeSpan averageRunTime)
		{
			Contract.Requires(averageRunTime, "averageRunTime").ToBeInRange(x => x >= TimeSpan.Zero);

			this.m_actualsWrapper.AverageRunTime = averageRunTime;
		}

		#endregion Actuals


		protected override void InitializeCustomData()
		{
			// Actuals
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
