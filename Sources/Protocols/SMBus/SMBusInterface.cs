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


		public Task QuickCommand(uint address)
		{
			return this.Bus.Send(address, new byte[0]);
		}

		private Task SendByte(uint address, byte data)
		{
			return this.Bus.Send(address, new[] { data });
		}

		public Task WriteByteCommand(uint address, uint commandId, byte data)
		{
			return this.Bus.Send(address, new[] { (byte)commandId, data });
		}

		public Task WriteWordCommand(uint address, uint commandId, ushort data)
		{
			var buffer = new[] { (byte)commandId, (byte)data, (byte)(data >> 8) };
			return this.Bus.Send(address, buffer);
		}

		public Task WriteBlockCommand(uint address, uint commandId, byte[] data)
		{
			var buffer = new byte[data.Length + 1];
			buffer[0] = (byte)commandId;
			Array.Copy(data, 0, buffer, 1, data.Length);

			return this.Bus.Send(address, buffer);
		}


		private Task<byte> ReceiveByte(uint address)
		{
			return this.Bus.Receive(address, 1)
				.ContinueWith(t => HandleReadTask(t, x => ConvertValue<byte>(x, 1)));
		}

		public Task<byte> ReadByteCommand(uint address, uint commandId)
		{
			var sendBuffer = new[] { (byte)commandId };

			return this.Bus.Transceive(address, sendBuffer, 1)
				.ContinueWith(t => HandleReadTask(t, x => ConvertValue<byte>(x, 1)));
		}

		public Task<UInt16> ReadWordCommand(uint address, uint commandId)
		{
			var sendBuffer = new[] { (byte)commandId };

			return this.Bus.Transceive(address, sendBuffer, 2)
				.ContinueWith(t => HandleReadTask(t, x => ConvertValue<UInt16>(x, 2)));
		}

		public Task<byte[]> ReadBlockCommand(uint address, uint commandId, int blockSize)
		{
			Contract.Requires(blockSize, "blockSize").IsInRange(x => x > 0);

			var sendBuffer = new[] { (byte)commandId };
			return this.Bus.Transceive(address, sendBuffer, blockSize);
		}

		private static TResult HandleReadTask<TInput, TResult>(Task<TInput> readTask, Func<TInput, TResult> convertFunc)
		{
			switch (readTask.Status)
			{
			case TaskStatus.RanToCompletion:
				var data = readTask.Result;
				return convertFunc(data);

			case TaskStatus.Canceled:
				throw new OperationCanceledException();

			case TaskStatus.Faulted:
				throw readTask.Exception;

			default:
				throw new InvalidOperationException("The task is not completed yet (Task Status: " + readTask.Status + ").");
			}
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
	}
}
