using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD2XX_NET;

namespace ImpruvIT.BatteryMonitor.Hardware.Ftdi
{
	/// <summary>
	/// A FTDI-based bus device discovery service.
	/// </summary>
	public class DeviceManagerDiscoveryService : IDiscoverDevices
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DeviceManagerDiscoveryService"/> class.
		/// </summary>
		public DeviceManagerDiscoveryService()
		{
			this.DeviceManager = new FTDI();
		}

		/// <summary>
		/// Gets the FTDI device manager. 
		/// </summary>
		protected FTDI DeviceManager { get; private set; }

		/// <summary>
		/// Gets all available FTDI-based bus devices.
		/// </summary>
		/// <returns>A list of <see cref="ICommunicateToBus">devices</see> communicating to a bus.</returns>
		public Task<IEnumerable<ICommunicateToBus>> GetConnectors()
		{
			return Task.Factory.StartNew(() =>
			{
				FTDI.FT_STATUS status;

				// Determine the number of FTDI devices connected to the machine
				uint ftdiDeviceCount = 0;
				status = this.DeviceManager.GetNumberOfDevices(ref ftdiDeviceCount);
				if (status != FTDI.FT_STATUS.FT_OK)
					throw new Exception("Unable to retrieve number fo devices connected to computer. (Result: " + status + ")");

				// If no devices available, return
				if (ftdiDeviceCount == 0)
					yield break;

				// Populate a device list
				FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];
				status = DeviceManager.GetDeviceList(ftdiDeviceList);
				if (status != FTDI.FT_STATUS.FT_OK)
					throw new Exception("Unable to retrieve information about connected devices. (Result: " + status + ")");

				return ftdiDeviceList.Select(d => new FtdiSMBusConnector(d)).ToList();
			});
			
		}

		/// <summary>
		/// Occurs when list of SMBus connectors changes.
		/// </summary>
		public event EventHandler ConnectorsChanged;
	}
}
