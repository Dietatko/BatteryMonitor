using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using FTD2XX_NET;

namespace ImpruvIT.BatteryMonitor.Hardware.Ftdi.I2C
{
	public static class NativeMethods_I2C
	{
		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS I2C_GetNumChannels(out uint channelCount);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS I2C_GetChannelInfo(uint index, [Out] Ftdi.NativeMethods.FT_DEVICE_LIST_INFO_NODE chanelInfo);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS I2C_OpenChannel(uint index, out IntPtr handle);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS I2C_InitChannel(IntPtr handle, ChannelConfig config);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS I2C_DeviceRead(
			IntPtr handle, 
			uint deviceAddress, 
			uint sizeToTransfer, 
			[Out] byte[] buffer, 
			out uint sizeTransferred, 
			[MarshalAs(UnmanagedType.U4)] TransferOptions options);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS I2C_DeviceWrite(
			IntPtr handle, 
			uint deviceAddress, 
			uint sizeToTransfer, 
			[In] byte[] buffer, 
			out uint sizeTransferred, 
			[MarshalAs(UnmanagedType.U4)] TransferOptions options);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS I2C_CloseChannel(IntPtr handle);

		[StructLayout(LayoutKind.Sequential)]
		public class ChannelConfig
		{
			public ChannelConfig(ClockRate clockRate, byte latencyTimer, ConfigOptions options)
			{
				this.ClockRate = clockRate;
				this.LatencyTimer = latencyTimer;
				this.Options = options;
			}

			public ClockRate ClockRate;

			public byte LatencyTimer;

			[MarshalAs(UnmanagedType.U4)]
			public ConfigOptions Options;
		}

		public enum ClockRate
		{
			/// <summary>
			/// 100 kb/sec
			/// </summary>
			Standard = 100000,

			/// <summary>
			/// 400 kb/sec
			/// </summary>
			Fast = 400000,

			/// <summary>
			/// 1 Mb/sec
			/// </summary>
			FastPlus = 1000000,

			/// <summary>
			/// 3.4 Mb/sec
			/// </summary>
			HighSpeed = 3400000
		}

		[Flags]
		public enum ConfigOptions : uint
		{
			None = 0,
			I2C_DISABLE_3PHASE_CLOCKING = 1,
			I2C_ENABLE_DRIVE_ONLY_ZERO = 2
		}

		[Flags]
		public enum TransferOptions : uint
		{
			None = 0,
			I2C_TRANSFER_OPTIONS_START_BIT = 1,
			I2C_TRANSFER_OPTIONS_STOP_BIT = 2,
			I2C_TRANSFER_OPTIONS_BREAK_ON_NACK = 4,
			I2C_TRANSFER_OPTIONS_NACK_LAST_BYTE = 8,
			I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES = 16,
			I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BITS = 32,
			I2C_TRANSFER_OPTIONS_FAST_TRANSFER = I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BYTES | I2C_TRANSFER_OPTIONS_FAST_TRANSFER_BITS,
			I2C_TRANSFER_OPTIONS_NO_ADDRESS = 64
		}
	}
}
