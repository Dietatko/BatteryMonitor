using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD2XX_NET;
using ImpruvIT.BatteryMonitor.Hardware.Ftdi.I2C;

namespace ImpruvIT.BatteryMonitor.Hardware.Ftdi
{
	/// <summary>
	/// A FTDI-based bus device connection.
	/// </summary>
	public class DeviceManagerConnection : IBusConnection
	{
		private const NativeMethods_I2C.TransferOptions WriteOptions =
			NativeMethods_I2C.TransferOptions.I2C_TRANSFER_OPTIONS_START_BIT |
			NativeMethods_I2C.TransferOptions.I2C_TRANSFER_OPTIONS_BREAK_ON_NACK;
		private const NativeMethods_I2C.TransferOptions ReadOptions =
			NativeMethods_I2C.TransferOptions.I2C_TRANSFER_OPTIONS_STOP_BIT |
			NativeMethods_I2C.TransferOptions.I2C_TRANSFER_OPTIONS_NACK_LAST_BYTE;

		/// <summary>
		/// Initializes a new instance of the <see cref="DeviceManagerConnection"/> class.
		/// </summary>
		public DeviceManagerConnection()
		{
			this.DeviceManager = new FTDI();
		}

		#region Connection

		/// <summary>
		/// A FTDi device manager.
		/// </summary>
		protected FTDI DeviceManager { get; private set; }

		/// <summary>
		/// A connected FTDI device info node.
		/// </summary>
		protected FTDI.FT_DEVICE_INFO_NODE DeviceNode { get; set; }

		/// <summary>
		/// Gets a value whether connection is connected.
		/// </summary>
		public bool IsConnected { get { return this.DeviceManager.IsOpen; } }

		protected IntPtr ChannelHandle { get; set; }


		/// <summary>
		/// Connects to the FTDI device.
		/// </summary>
		/// <param name="deviceNode">A FTDI device info node to connect.</param>
		public virtual void Connect(FTDI.FT_DEVICE_INFO_NODE deviceNode)
		{
			this.DeviceNode = deviceNode;

			// Open device by serial number
			FTDI.FT_STATUS status = this.DeviceManager.OpenBySerialNumber(this.DeviceNode.SerialNumber);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Unable to connect to FTDI device with serial number '" + this.DeviceNode.SerialNumber + "' (Result: '" + status + "').");
			}

			//// Set up device data parameters
			//// Set Baud rate to 9600
			//ftStatus = myFtdiDevice.SetBaudRate(9600);
			//if (ftStatus != FTDI.FT_STATUS.FT_OK)
			//{
			//	// Wait for a key press
			//	Console.WriteLine("Failed to set Baud rate (error " + ftStatus.ToString() + ")");
			//	Console.ReadKey();
			//	return;
			//}

			//// Set data characteristics - Data bits, Stop bits, Parity
			//ftStatus = myFtdiDevice.SetDataCharacteristics(FTDI.FT_DATA_BITS.FT_BITS_8, FTDI.FT_STOP_BITS.FT_STOP_BITS_1, FTDI.FT_PARITY.FT_PARITY_NONE);
			//if (ftStatus != FTDI.FT_STATUS.FT_OK)
			//{
			//	// Wait for a key press
			//	Console.WriteLine("Failed to set data characteristics (error " + ftStatus.ToString() + ")");
			//	Console.ReadKey();
			//	return;
			//}

			//// Set flow control - set RTS/CTS flow control
			//ftStatus = myFtdiDevice.SetFlowControl(FTDI.FT_FLOW_CONTROL.FT_FLOW_RTS_CTS, 0x11, 0x13);
			//if (ftStatus != FTDI.FT_STATUS.FT_OK)
			//{
			//	// Wait for a key press
			//	Console.WriteLine("Failed to set flow control (error " + ftStatus.ToString() + ")");
			//	Console.ReadKey();
			//	return;
			//}

			//// Set read timeout to 5 seconds, write timeout to infinite
			//ftStatus = myFtdiDevice.SetTimeouts(5000, 0);
			//if (ftStatus != FTDI.FT_STATUS.FT_OK)
			//{
			//	// Wait for a key press
			//	Console.WriteLine("Failed to set timeouts (error " + ftStatus.ToString() + ")");
			//	Console.ReadKey();
			//	return;
			//}

			//// Perform loop back - make sure loop back connector is fitted to the device
			//// Write string data to the device
			//string dataToWrite = "Hello world!";
			//UInt32 numBytesWritten = 0;
			//// Note that the Write method is overloaded, so can write string or byte array data
			//ftStatus = myFtdiDevice.Write(dataToWrite, dataToWrite.Length, ref numBytesWritten);
			//if (ftStatus != FTDI.FT_STATUS.FT_OK)
			//{
			//	// Wait for a key press
			//	Console.WriteLine("Failed to write to device (error " + ftStatus.ToString() + ")");
			//	Console.ReadKey();
			//	return;
			//}


			//// Check the amount of data available to read
			//// In this case we know how much data we are expecting, 
			//// so wait until we have all of the bytes we have sent.
			//UInt32 numBytesAvailable = 0;
			//do
			//{
			//	ftStatus = myFtdiDevice.GetRxBytesAvailable(ref numBytesAvailable);
			//	if (ftStatus != FTDI.FT_STATUS.FT_OK)
			//	{
			//		// Wait for a key press
			//		Console.WriteLine("Failed to get number of bytes available to read (error " + ftStatus.ToString() + ")");
			//		Console.ReadKey();
			//		return;
			//	}
			//	Thread.Sleep(10);
			//} while (numBytesAvailable < dataToWrite.Length);

			//// Now that we have the amount of data we want available, read it
			//string readData;
			//UInt32 numBytesRead = 0;
			//// Note that the Read method is overloaded, so can read string or byte array data
			//ftStatus = myFtdiDevice.Read(out readData, numBytesAvailable, ref numBytesRead);
			//if (ftStatus != FTDI.FT_STATUS.FT_OK)
			//{
			//	// Wait for a key press
			//	Console.WriteLine("Failed to read data (error " + ftStatus.ToString() + ")");
			//	Console.ReadKey();
			//	return;
			//}
		}

		/// <summary>
		/// Disconnects from device.
		/// </summary>
		public Task Disconnect()
		{
			return Task.Factory.StartNew(() =>
			{
				// Close our device
				var ftStatus = this.DeviceManager.Close();
				if (ftStatus != FTDI.FT_STATUS.FT_OK)
					throw new InvalidOperationException("Unable to disconnect from FTDI device with serial number '" + this.DeviceNode.SerialNumber + "' (Result: '" + ftStatus + "').");

				this.DeviceNode = null;
			});
		}

		#endregion Connection


		public void QuickCommand(uint address)
		{
			throw new NotImplementedException();
		}

		public byte ReceiveByte(uint address)
		{
			throw new NotImplementedException();
		}

		public byte ReadByteCommand(uint address, uint commandId)
		{
			byte[] data = new byte[1];
			uint sizeTransferred;

			data[0] = (byte)commandId;
			var status = NativeMethods_I2C.I2C_DeviceWrite(this.ChannelHandle, address, 1, data, out sizeTransferred, WriteOptions);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Error while writing to SMBUs. (Status: " + status + ")");
			}

			status = NativeMethods_I2C.I2C_DeviceRead(this.ChannelHandle, address, 1, data, out sizeTransferred, ReadOptions);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Error while reading from SMBUs. (Status: " + status + ")");
			}

			return data[0];
		}

		public ushort ReadWordCommand(uint address, uint commandId)
		{
			throw new NotImplementedException();
		}

		public byte[] ReadBlockCommand(uint address, uint commandId, int blockSize)
		{
			throw new NotImplementedException();
		}

		public void SendByte(uint address, byte data)
		{
			throw new NotImplementedException();
		}

		public void WriteByteCommand(uint address, uint commandId, byte data)
		{
			throw new NotImplementedException();
		}

		public void WriteWordCommand(uint address, uint commandId, ushort data)
		{
			throw new NotImplementedException();
		}

		public void WriteBlockCommand(uint address, uint commandId, byte[] data)
		{
			throw new NotImplementedException();
		}
	}
}
