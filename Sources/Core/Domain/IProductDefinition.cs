using System;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public interface IProductDefinition
	{
		string Manufacturer { get; }
		string Product { get; }
		string Chemistry { get; }
		DateTime ManufactureDate { get; }
		string SerialNumber { get; }
	}
}