using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ImpruvIT.Contracts;
using ImpruvIT.BatteryMonitor.Hardware;

namespace ImpruvIT.BatteryMonitor.Protocols.LinearTechnology.LTC6804
{
	public class LTC6804_1Interface
	{
		public LTC6804_1Interface(ICommunicateToBus connection, int chainLength)
		{
			Contract.Requires(connection, "connection").NotToBeNull();
			Contract.Requires(chainLength, "chainLength").ToBeInRange(x => 0 < x);

			this.Connection = connection;
			this.ChainLength = chainLength;
		}

		public ICommunicateToBus Connection { get; set; }
		public int ChainLength { get; }

		public Task WakeUp()
		{
			return this.WriteData(new byte[1]);
		}

		public Task ExecuteCommand(ushort commandId)
		{
			// Create buffer
			var buffer = new byte[4];

			// Add command data
			buffer[0] = (byte)(commandId >> 8);
			buffer[1] = (byte)commandId;
			AddPec(buffer, 0, 2);

			// Write data to bus
			return this.WriteData(buffer);
		}

		public Task WriteRegister(ushort commandId, byte[] chipData)
		{
			return this.WriteRegister(commandId, Enumerable.Repeat(chipData, this.ChainLength));
		}

		public Task WriteRegister(ushort commandId, IEnumerable<byte[]> chainData)
		{
			chainData = chainData.ToList();
			var dataLength = chainData.Sum(x => x.Length);

			// Create buffer
			var bufferSize = 2 + 2 + dataLength + chainData.Count() * 2;
			var buffer = new byte[bufferSize];

			// Add command data
			buffer[0] = (byte)(commandId >> 8);
			buffer[1] = (byte)commandId;
			AddPec(buffer, 0, 2);

			// Add chain data
			var bufferIndex = 4;
			foreach (var data in chainData)
			{
				Array.Copy(data, 0, buffer, bufferIndex, data.Length);
				AddPec(buffer, bufferIndex, data.Length);
				bufferIndex += data.Length + 2;
			}

			// Write data to bus
			return this.WriteData(buffer);
		}

		public async Task<IEnumerable<byte[]>> ReadRegister(ushort commandId, int dataLength)
		{
			// Create buffer
			var outBuffer = new byte[4];

			// Add command data
			outBuffer[0] = (byte)(commandId >> 8);
			outBuffer[1] = (byte)commandId;
			AddPec(outBuffer, 0, 2);

			// Write data to bus
			var receiveLength = this.ChainLength * (dataLength + 2);
			var inBuffer = await this.TransceiveData(outBuffer, receiveLength).ConfigureAwait(false);
			if (inBuffer.Length != receiveLength)
				return null;

			var chainData = new List<byte[]>(this.ChainLength);

			var inBufferIndex = 0;
			for (int i = 0; i < this.ChainLength; i++)
			{
				byte[] icData = null;

				if (this.CheckPec(inBuffer, inBufferIndex, dataLength))
				{
					icData = new byte[dataLength];
					Array.Copy(inBuffer, inBufferIndex, icData, 0, dataLength);
				}

				chainData.Add(icData);
				inBufferIndex += dataLength + 2;
			}

			return chainData; 
		}

		private void AddPec(byte[] buffer, int startIndex, int byteCount)
		{
			var pec = CalculatePec(buffer, startIndex, byteCount);
			buffer[startIndex + byteCount] = (byte)(pec >> 8);
			buffer[startIndex + byteCount + 1] = (byte)pec;
		}

		private bool CheckPec(byte[] buffer, int startIndex, int byteCount)
		{
			var expectedPec = CalculatePec(buffer, startIndex, byteCount);
			var actualPec = (ushort)((buffer[startIndex + byteCount] << 8) | buffer[startIndex + byteCount + 1]);

			return expectedPec == actualPec;
		}

		private ushort CalculatePec(byte[] buffer, int startIndex, int byteCount)
		{
			ushort pec = 0x0010;
			const ushort shiftMask = 0x3A66;

			for (var i = 0; i < byteCount; i++)
			{
				var bufferByte = buffer[startIndex + i];

				for (var b = 0; b < 8; b++)
				{
					var din = (bufferByte & 0x80) >> 7;

					var in0 = din ^ ((pec >> 14) & 1);
					var in3 = in0 ^ ((pec >> 2) & 1);
					var in4 = in0 ^ ((pec >> 3) & 1);
					var in7 = in0 ^ ((pec >> 6) & 1);
					var in8 = in0 ^ ((pec >> 7) & 1);
					var in10 = in0 ^ ((pec >> 9) & 1);
					var in14 = in0 ^ ((pec >> 13) & 1);
					var inValue = (in14 << 14)
						| (in10 << 10)
						| (in8 << 8)
						| (in7 << 7)
						| (in4 << 4)
						| (in3 << 3)
						| (in0 << 0);

					pec <<= 1;
					pec &= shiftMask;
					pec |= (ushort)inValue;

					bufferByte <<= 1;
				}
			}

			pec <<= 1;
			return pec;
		}

		private Task WriteData(byte[] buffer)
		{
			return this.Connection.Send(buffer);
		}

		private Task<byte[]> TransceiveData(byte[] outBuffer, int receiveLength)
		{
			return this.Connection.Transceive(outBuffer, receiveLength);
		}
	}
}
