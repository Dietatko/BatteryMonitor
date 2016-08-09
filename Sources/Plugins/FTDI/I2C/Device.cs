using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImpruvIT.BatteryMonitor.Hardware.Ftdi.I2C
{
	/// <summary>
	/// A FTDI-based bus device using the MPSSE mode.
	/// </summary>
	public class Device : MpsseDevice
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Device"/> class.
		/// </summary>
		/// <param name="deviceNode">A FTDI device info node.</param>
		/// <param name="deviceChannelIndex">A channel index.</param>
		public Device(NativeMethods.FT_DEVICE_LIST_INFO_NODE deviceNode, int deviceChannelIndex)
			: base(deviceNode, deviceChannelIndex)
		{
		}

		/// <inheritdoc />
		public override string Type
		{
			get { return "FTDI MPSEE I2C"; }
		}

		/// <inheritdoc />
		public override Task<IBusConnection> Connect()
		{
			var connection = new I2C.Connection();
			var task = connection.Connect(this.DeviceNode.SerialNumber, this.DeviceChannelIndex);

			return task.ContinueWith<IBusConnection>(x => connection);
		}
	}
}
