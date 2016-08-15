namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public interface IDesignParameters
	{
		float NominalVoltage { get; }
		float DesignedDischargeCurrent { get; }
		float MaxDischargeCurrent { get; }
		float DesignedCapacity { get; }
	}
}