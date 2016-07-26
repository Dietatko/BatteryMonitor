using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImpruvIT.BatteryMonitor.Hardware
{
	/// <summary>
	/// Denotes a hardware bus device.
	/// </summary>
	public interface IBusDevice
	{
		/// <summary>
		/// Gets a user readable name of the device.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Gets a user readable type of the device.
		/// </summary>
		string Type { get; }

		/// <summary>
		/// Gets a list of custom properties of the connector.
		/// </summary>
		IDictionary<string, string> Properties { get; }

		/// <summary>
		/// Connects to the bus.
		/// </summary>
		/// <returns>A completion task providing connection to the bus.</returns>
		Task<IBusConnection> Connect();
	}
}
