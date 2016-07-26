using System;
using System.Threading.Tasks;

namespace ImpruvIT.BatteryMonitor.Hardware
{
	public interface ICommunicateToBus
	{
		Task Send(byte[] data);
		Task<byte[]> Receive(int dataLength);
		Task<byte[]> Transceive(byte[] dataToSend, int receiveLength);
	}
}
