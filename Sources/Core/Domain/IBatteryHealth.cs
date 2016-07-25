namespace ImpruvIT.BatteryMonitor.Domain
{
	public interface IBatteryHealth
	{
		float FullChargeCapacity { get; }
		int CycleCount { get; }
		float CalculationPrecision { get; }
	}
}