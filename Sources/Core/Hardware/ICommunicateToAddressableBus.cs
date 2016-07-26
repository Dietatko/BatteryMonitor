using System;
using System.Threading.Tasks;

namespace ImpruvIT.BatteryMonitor.Hardware
{
	public interface ICommunicateToAddressableBus
	{
		Task Send(uint address, byte[] data);
		Task<byte[]> Receive(uint address, int dataLength);
		Task<byte[]> Transceive(uint address, byte[] dataToSend, int receiveLength);
	}
}
