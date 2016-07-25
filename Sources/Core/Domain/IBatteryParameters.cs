namespace ImpruvIT.BatteryMonitor.Domain
{
	public interface IBatteryParameters
	{
		float NominalVoltage { get; }
		float DesignedDischargeCurrent { get; }
		float MaxDischargeCurrent { get; }
		float DesignedCapacity { get; }
	}
}