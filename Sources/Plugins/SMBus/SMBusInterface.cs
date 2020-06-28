using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ImpruvIT.BatteryMonitor.Hardware;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Protocols.SMBus
{
	public class SMBusInterface
	{
		public SMBusInterface(ICommunicateToAddressableBus bus)
		{
			Contract.Requires(bus, "bus");

			this.Bus = bus;
		}

		public ICommunicateToAddressableBus Bus { get; private set; }


		public Task QuickCommand(byte address)
		{
			return this.Bus.Send(address, new byte[0]);
		}

		public Task SendByte(byte address, byte data)
        {
            var buffer = new byte[3];
            buffer[0] = GetAddressByte(address, false);
            buffer[1] = data;
            buffer[2] = PecCalculator.CalculatePec(buffer, 0, 2);

            var sendBuffer = new byte[2];
            Array.Copy(buffer, 1, sendBuffer, 0, sendBuffer.Length);

            return Bus.Send(address, sendBuffer);
		}

		public Task WriteByteCommand(byte address, byte commandId, byte data)
		{
			return WriteValueCommand(address, commandId, new[] { data });
		}

		public Task WriteWordCommand(byte address, byte commandId, ushort data)
        {
            return WriteValueCommand(address, commandId, new[] { (byte)data, (byte)(data >> 8) });
		}

		public Task WriteBlockCommand(byte address, byte commandId, byte[] data)
        {
            return WriteValueCommand(address, commandId, data);
        }

        private Task WriteValueCommand(byte address, byte commandId, byte[] data)
        {
            var buffer = new byte[2 + data.Length + 1];
            buffer[0] = GetAddressByte(address, false);
            buffer[1] = commandId;
            data.CopyTo(buffer, 2);
            buffer[2 + data.Length] = PecCalculator.CalculatePec(buffer, 0, 2 + data.Length);

            var sendBuffer = new byte[1 + data.Length + 1];
            Array.Copy(buffer, 1, sendBuffer, 0, sendBuffer.Length);

            return Bus.Send(address, sendBuffer);
        }

        public async Task<byte> ReceiveByte(byte address)
        {
            var buffer = new byte[3];
            buffer[0] = GetAddressByte(address, true);

            var receiveBuffer = await Bus.Receive(address, 2);
            receiveBuffer.CopyTo(buffer, 1);

            VerifyPec(buffer, 0, buffer.Length);

            return receiveBuffer[0];
        }

        public async Task<byte> ReadByteCommand(byte address, byte commandId)
        {
            var data = await ReadValueCommand(
                address, 
                commandId, 
                1,
                _ => 1);

            return ConvertValue<byte>(data, 1);
        }

		public async Task<ushort> ReadWordCommand(byte address, byte commandId)
		{
            var data = await ReadValueCommand(
                address, 
                commandId, 
                2,
                _ => 2);

            return ConvertValue<ushort>(data, 2);
        }

        public async Task<byte[]> ReadBlockCommand(byte address, byte commandId, int blockSize)
		{
			Contract.Requires(blockSize, "blockSize").ToBeInRange(x => x > 0);

            var data = await ReadValueCommand(
                address,
                commandId,
                blockSize,
                x => x[0] + 1);

            var resultBuffer = new byte[data.Length - 1];
            Array.Copy(data, 1, resultBuffer, 0, resultBuffer.Length);
            return resultBuffer;
        }

        private async Task<byte[]> ReadValueCommand(
            byte address, 
            byte commandId, 
            int maxDataLength,
            Func<byte[], int> valueLengthFunc)
        {
            var buffer = new byte[3 + maxDataLength + 1];
            buffer[0] = GetAddressByte(address, false);
            buffer[1] = commandId;
            buffer[2] = GetAddressByte(address, true);

            var sendBuffer = new[] { commandId };
            var receiveBuffer = await Bus.Transceive(address, sendBuffer, maxDataLength + 1);

            var actualDataLength = valueLengthFunc(receiveBuffer);

            Array.Copy(receiveBuffer, 0, buffer, 3, actualDataLength + 1);
            VerifyPec(buffer, 0, 3 + actualDataLength + 1);

            var resultBuffer = new byte[actualDataLength];
            Array.Copy(receiveBuffer, 0, resultBuffer, 0, resultBuffer.Length);
            return resultBuffer;
        }

		private static T ConvertValue<T>(byte[] data, int byteCount)
		{
			if (data.Length < byteCount)
				throw new InvalidOperationException(String.Format("Not enough data received from the bus. Expected {0} byte, while {1} was received.", byteCount, data.Length));

			uint result = 0;
			while (byteCount > 0)
			{
				result <<= 8;
				result += data[byteCount - 1];

				byteCount--;
			}

			return (T)Convert.ChangeType(result, typeof(T));
		}


        private static byte GetAddressByte(byte address, bool read)
        {
            return (byte)((address << 1) | (read ? (byte)1 : 0));
        }

        private static void VerifyPec(byte[] buffer, int start, int length)
        {
            if (buffer.Length < start + length)
                throw new InvalidOperationException("The PEC byte is missing in source data.");

            if (PecCalculator.CalculatePec(buffer, start, length) != 0)
                throw new CommunicationException("The Packet Error Code check failed.");
        }
    }
}
