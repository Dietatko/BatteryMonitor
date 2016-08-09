using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Hardware.Ftdi
{
	public class MpsseDiscoveryService : SequentialDiscoveryService
	{
		public MpsseDiscoveryService()
			: base(new IDiscoverDevices[] {
					//new I2C.DiscoveryService(),
					new SPI.DiscoveryService()
				})
		{
		}
	}
}
