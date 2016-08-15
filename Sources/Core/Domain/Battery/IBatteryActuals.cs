using System;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public interface IBatteryActuals
	{
		float Voltage { get; }
		float ActualCurrent { get; }
		float AverageCurrent { get; }
		float Temperature { get; }

		float RemainingCapacity { get; }
		float AbsoluteStateOfCharge { get; }
		float RelativeStateOfCharge { get; }

		TimeSpan ActualRunTime { get; }
		TimeSpan AverageRunTime { get; }
	}
}