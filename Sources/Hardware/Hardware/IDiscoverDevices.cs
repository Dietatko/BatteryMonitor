using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImpruvIT.BatteryMonitor.Hardware
{
	/// <summary>
	/// Denotes a bus communication device discovery service.
	/// </summary>
	public interface IDiscoverDevices
	{
		/// <summary>
		/// Gets all available bus devices.
		/// </summary>
		/// <returns>A list of <see cref="IBusDevice">devices</see> communicating to a bus.</returns>
		Task<IEnumerable<IBusDevice>> GetConnectors();

		/// <summary>
		/// Occurs when list of bus devices changes.
		/// </summary>
		event EventHandler ConnectorsChanged;
	}
}
