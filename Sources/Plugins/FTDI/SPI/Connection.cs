using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FTD2XX_NET;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Hardware.Ftdi.SPI
{
	/// <summary>
	/// A FTDI-based SMBus device connection.
	/// </summary>
	public class Connection : IBusConnection, ICommunicateToBus
	{
		private const SPI.NativeMethods_SPI.TransferOptions WriteOptions =
			SPI.NativeMethods_SPI.TransferOptions.SPI_TRANSFER_OPTIONS_SIZE_IN_BYTES |
			SPI.NativeMethods_SPI.TransferOptions.SPI_TRANSFER_OPTIONS_CHIPSELECT_ENABLE;

		private const SPI.NativeMethods_SPI.TransferOptions ReadOptions =
			SPI.NativeMethods_SPI.TransferOptions.SPI_TRANSFER_OPTIONS_SIZE_IN_BYTES |
			SPI.NativeMethods_SPI.TransferOptions.SPI_TRANSFER_OPTIONS_CHIPSELECT_ENABLE |
			SPI.NativeMethods_SPI.TransferOptions.SPI_TRANSFER_OPTIONS_CHIPSELECT_DISABLE;

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

				NativeMethods.Init_libMPSSE();

				// Find requested channel
				uint channelCount;
				status = SPI.NativeMethods_SPI.SPI_GetNumChannels(out channelCount);
				if (status != FTDI.FT_STATUS.FT_OK)
					throw new InvalidOperationException("Unable to find number of SPI channels. (Status: " + status + ")");

				uint channelIndex;
				var deviceNode = new NativeMethods.FT_DEVICE_LIST_INFO_NODE();
				var tmpChannelIndex = deviceChannelIndex;
				for (channelIndex = 0; channelIndex < channelCount; channelIndex++)
				{
					status = SPI.NativeMethods_SPI.SPI_GetChannelInfo(0, deviceNode);
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
				status = SPI.NativeMethods_SPI.SPI_OpenChannel(channelIndex, out handle);
				if (status != FTDI.FT_STATUS.FT_OK)
					throw new InvalidOperationException("Unable to open SPI channel. (Status: " + status + ")");

				this.ChannelHandle = handle;

				// Configure channel
				var configOptions = SPI.NativeMethods_SPI.ConfigOptions.SPI_CONFIG_OPTION_MODE0 |
				                    SPI.NativeMethods_SPI.ConfigOptions.SPI_CONFIG_OPTION_CS_DBUS3 |
				                    SPI.NativeMethods_SPI.ConfigOptions.SPI_CONFIG_OPTION_CS_ACTIVELOW;
				var config = new SPI.NativeMethods_SPI.ChannelConfig(250000, 2, configOptions, 0);
				status = SPI.NativeMethods_SPI.SPI_InitChannel(this.ChannelHandle, config);
				if (status != FTDI.FT_STATUS.FT_OK)
					throw new InvalidOperationException("Unable to initialize SPI channel. (Status: " + status + ")");
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
				var status = SPI.NativeMethods_SPI.SPI_CloseChannel(this.ChannelHandle);
				this.DeviceNode = null;
				this.ChannelHandle = IntPtr.Zero;
				if (status != FTDI.FT_STATUS.FT_OK)
					throw new InvalidOperationException("Unable to close I2C channel. (Status: " + status + ")");
			});
		}

		#endregion Connection

		public Task Send(byte[] data)
		{
			Contract.Requires(data, "data")
				.NotToBeNull();

			return Task.Run(() =>
			{
				uint transferredSize;

				var status = SPI.NativeMethods_SPI.SPI_Write(
					this.ChannelHandle, 
					data, 
					(uint)data.Length, 
					out transferredSize,
					WriteOptions | SPI.NativeMethods_SPI.TransferOptions.SPI_TRANSFER_OPTIONS_CHIPSELECT_DISABLE);

				if (status != FTDI.FT_STATUS.FT_OK || transferredSize != data.Length)
					throw new InvalidOperationException("Error while writing to the bus. (Status: " + status + ")");
			});
		}

		public Task<byte[]> Receive(int dataLength)
		{
			Contract.Requires(dataLength, "dataLength")
				.ToBeInRange(x => x > 0);

			return Task.Run(() =>
			{
				var buffer = new byte[dataLength];
				uint transferredSize;

				var status = SPI.NativeMethods_SPI.SPI_Read(
					this.ChannelHandle,
					buffer,
					(uint)dataLength,
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

		public Task<byte[]> Transceive(byte[] dataToSend, int receiveLength)
		{
			Contract.Requires(dataToSend, "dataToSend").NotToBeNull();
			Contract.Requires(receiveLength, "receiveLength").ToBeInRange(x => x > 0);

			return Task.Run(() =>
			{
				uint transferredSize;

				// Send data
				var status = SPI.NativeMethods_SPI.SPI_Write(
					this.ChannelHandle,
					dataToSend,
					(uint)dataToSend.Length,
					out transferredSize,
					WriteOptions);

				if (status != FTDI.FT_STATUS.FT_OK || transferredSize != dataToSend.Length)
					throw new InvalidOperationException("Error while writing to the bus. (Status: " + status + ")");

				// Receive data
				var buffer = new byte[receiveLength];

				status = SPI.NativeMethods_SPI.SPI_Read(
					this.ChannelHandle,
					buffer,
					(uint)receiveLength,
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
