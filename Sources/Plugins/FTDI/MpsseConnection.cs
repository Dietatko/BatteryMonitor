using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FTD2XX_NET;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Hardware.Ftdi
{
	/// <summary>
	/// A FTDI-based SMBus device connection.
	/// </summary>
	public class MpsseConnection : IBusConnection, ICommunicateToAddressableBus
	{
		private const NativeMethods.TransferOptions WriteOptions =
			NativeMethods.TransferOptions.I2C_TRANSFER_OPTIONS_START_BIT |
			NativeMethods.TransferOptions.I2C_TRANSFER_OPTIONS_BREAK_ON_NACK;

		private const NativeMethods.TransferOptions ReadOptions =
			NativeMethods.TransferOptions.I2C_TRANSFER_OPTIONS_START_BIT
			| NativeMethods.TransferOptions.I2C_TRANSFER_OPTIONS_STOP_BIT;

		#region Connection

		/// <summary>
		/// A connected FTDI device info node.
		/// </summary>
		protected NativeMethods.FT_DEVICE_LIST_INFO_NODE DeviceNode { get; set; }

		protected IntPtr ChannelHandle { get; set; }

		/// <summary>
		/// Gets a value whether connection is connected.
		/// </summary>
		public bool IsConnected { get { return this.ChannelHandle != IntPtr.Zero; } }

		/// <summary>
		/// Connects to the FTDI device.
		/// </summary>
		/// <param name="serialNumber">A FTDI device serial number.</param>
		/// <param name="deviceChannelIndex">A channel index in the device.</param>
		public virtual Task Connect(string serialNumber, int deviceChannelIndex)
		{
			return Task.Run(() =>
			{
				FTDI.FT_STATUS status;

				// Find requested channel
				uint channelCount;
				status = NativeMethods.I2C_GetNumChannels(out channelCount);
				if (status != FTDI.FT_STATUS.FT_OK)
					throw new InvalidOperationException("Unable to find number of I2C channels. (Status: " + status + ")");

				uint channelIndex;
				var deviceNode = new NativeMethods.FT_DEVICE_LIST_INFO_NODE();
				var tmpChannelIndex = deviceChannelIndex;
				for (channelIndex = 0; channelIndex < channelCount; channelIndex++)
				{
					status = NativeMethods.I2C_GetChannelInfo(0, deviceNode);
					if (status != FTDI.FT_STATUS.FT_OK)
						throw new InvalidOperationException("Unable to get information about channel " + channelIndex + ". (Status: " + status + ")");

					if (deviceNode.SerialNumber == serialNumber)
					{
						if (tmpChannelIndex > 0)
							tmpChannelIndex--;
						else
							break;
					}
				}

				if (channelIndex >= channelCount)
					throw new InvalidOperationException("Unable to find channel " + deviceChannelIndex + " on device with serial number '" + serialNumber + "'.");

				this.DeviceNode = deviceNode;

				// Open channel
				IntPtr handle;
				status = NativeMethods.I2C_OpenChannel(channelIndex, out handle);
				if (status != FTDI.FT_STATUS.FT_OK)
					throw new InvalidOperationException("Unable to open I2C channel. (Status: " + status + ")");

				this.ChannelHandle = handle;

				// Configure channel
				// NativeMethods.ClockRate.Standard
				var config = new NativeMethods.ChannelConfig((NativeMethods.ClockRate)25000, 1, NativeMethods.ConfigOptions.I2C_ENABLE_DRIVE_ONLY_ZERO);
				status = NativeMethods.I2C_InitChannel(this.ChannelHandle, config);
				if (status != FTDI.FT_STATUS.FT_OK)
					throw new InvalidOperationException("Unable to initialize I2C channel. (Status: " + status + ")");
			});
		}

		/// <summary>
		/// Disconnects from device.
		/// </summary>
		public Task Disconnect()
		{
			if (this.ChannelHandle == IntPtr.Zero)
				return Task.CompletedTask;

			// Close the channel
			return Task.Run(() =>
			{
				var status = NativeMethods.I2C_CloseChannel(this.ChannelHandle);
				this.DeviceNode = null;
				this.ChannelHandle = IntPtr.Zero;
				if (status != FTDI.FT_STATUS.FT_OK)
					throw new InvalidOperationException("Unable to close I2C channel. (Status: " + status + ")");
			});
		}

		#endregion Connection

		public Task Send(uint address, byte[] data)
		{
			Contract.Requires(data, "data")
				.IsNotNull();

			return Task.Run(() =>
			{
				uint transferredSize;

				var status = NativeMethods.I2C_DeviceWrite(
					this.ChannelHandle, 
					address, 
					(uint)data.Length, 
					data, 
					out transferredSize, 
					WriteOptions | NativeMethods.TransferOptions.I2C_TRANSFER_OPTIONS_STOP_BIT);

				if (status != FTDI.FT_STATUS.FT_OK || transferredSize != data.Length)
					throw new InvalidOperationException("Error while writing to the bus. (Status: " + status + ")");
			});
		}

		public Task<byte[]> Receive(uint address, int dataLength)
		{
			Contract.Requires(dataLength, "dataLength")
				.IsInRange(x => x > 0);

			return Task.Run(() =>
			{
				var buffer = new byte[dataLength];
				uint transferredSize;
				
				var status = NativeMethods.I2C_DeviceRead(
					this.ChannelHandle,
					address,
					(uint)dataLength,
					buffer,
					out transferredSize,
					ReadOptions);

				if (status != FTDI.FT_STATUS.FT_OK)
					throw new InvalidOperationException("Error while reading from the bus. (Status: " + status + ")");

				if (transferredSize < dataLength)
				{
					// If less bytes receive as expected => Copy to smaller array 
					var tmpData = new byte[transferredSize];
					Array.Copy(buffer, tmpData, transferredSize);
					buffer = tmpData;
				}

				return buffer;
			});
		}

		public Task<byte[]> Transceive(uint address, byte[] dataToSend, int receiveLength)
		{
			Contract.Requires(dataToSend, "dataToSend").IsNotNull();
			Contract.Requires(receiveLength, "receiveLength").IsInRange(x => x > 0);

			return Task.Run(() =>
			{
				uint transferredSize;

				// Send data
				var status = NativeMethods.I2C_DeviceWrite(
					this.ChannelHandle,
					address,
					(uint)dataToSend.Length,
					dataToSend,
					out transferredSize,
					WriteOptions);

				if (status != FTDI.FT_STATUS.FT_OK || transferredSize != dataToSend.Length)
					throw new InvalidOperationException("Error while writing to the bus. (Status: " + status + ")");

				// Receive data
				var buffer = new byte[receiveLength];

				status = NativeMethods.I2C_DeviceRead(
					this.ChannelHandle,
					address,
					(uint)receiveLength,
					buffer,
					out transferredSize,
					ReadOptions);

				if (status != FTDI.FT_STATUS.FT_OK)
					throw new InvalidOperationException("Error while reading from the bus. (Status: " + status + ")");

				if (transferredSize < receiveLength)
				{
					// If less bytes receive as expected => Copy to smaller array 
					var tmpData = new byte[transferredSize];
					Array.Copy(buffer, tmpData, transferredSize);
					buffer = tmpData;
				}

				return buffer;
			});
		}
	}
}
