namespace ImpruvIT.BatteryMonitor.Domain
{
	public interface IDesignParameters
	{
		float NominalVoltage { get; }
		float DesignedDischargeCurrent { get; }
		float MaxDischargeCurrent { get; }
		float DesignedCapacity { get; }
	}
}