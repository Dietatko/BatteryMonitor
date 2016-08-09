using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FTD2XX_NET;

namespace ImpruvIT.BatteryMonitor.Hardware.Ftdi.SPI
{
	/// <summary>
	/// A FTDI-based bus device discovery service.
	/// </summary>
	public class DiscoveryService : IDiscoverDevices
	{
		/// <summary>
		/// Gets all available FTDI-based bus devices.
		/// </summary>
		/// <returns>A list of <see cref="IBusDevice">devices</see> communicating to a bus.</returns>
		public Task<IEnumerable<IBusDevice>> GetConnectors()
		{
			return Task.Factory.StartNew(() =>
			{
				FTDI.FT_STATUS status;

				// Determine the number of channels on all FTDI devices connected to the machine
				uint channelCount;
				status = SPI.NativeMethods_SPI.SPI_GetNumChannels(out channelCount);
				if (status != FTDI.FT_STATUS.FT_OK)
					throw new Exception("Unable to retrieve number of channels on devices connected to computer. (Result: " + status + ")");

				var allNodes = new List<NativeMethods.FT_DEVICE_LIST_INFO_NODE>((int)channelCount);
				for (int i = 0; i < channelCount; i++)
				{
					var deviceInfo = new NativeMethods.FT_DEVICE_LIST_INFO_NODE();
					status = SPI.NativeMethods_SPI.SPI_GetChannelInfo((uint)i, deviceInfo);
					if (status != FTDI.FT_STATUS.FT_OK)
					{
						throw new Exception("Unable to retrieve information about SPI channel. (Status: " + status + ")");
					}

					allNodes.Add(deviceInfo);
				}

				var devices = new List<IBusDevice>();
				foreach (var group in allNodes.GroupBy(x => x.SerialNumber))
				{
					int deviceChannelIndex = 0;
					foreach (var deviceInfo in group)
					{
						devices.Add(new I2C.Device(deviceInfo, deviceChannelIndex));
						deviceChannelIndex++;
					}
				}

				return (IEnumerable<IBusDevice>)devices;
			});
			
		}

		/// <summary>
		/// Occurs when list of SMBus connectors changes.
		/// </summary>
		public event EventHandler ConnectorsChanged;
	}
}
