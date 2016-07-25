using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpruvIT.BatteryMonitor.Hardware
{
	/// <summary>
	/// Denotes a hardware bus connection.
	/// </summary>
	public interface IBusConnection
	{
		/// <summary>
		/// Gets a value whether connection is connected.
		/// </summary>
		bool IsConnected { get; }

		/// <summary>
		/// Disconnects from the bus.
		/// </summary>
		/// <returns>The completion task.</returns>
		Task Disconnect();
	}
}
