namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public interface IBatteryHealth
	{
		float FullChargeCapacity { get; }
		int CycleCount { get; }
		float CalculationPrecision { get; }
	}
}