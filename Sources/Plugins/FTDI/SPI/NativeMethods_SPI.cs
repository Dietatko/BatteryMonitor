using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using FTD2XX_NET;

namespace ImpruvIT.BatteryMonitor.Hardware.Ftdi.SPI
{
	public static class NativeMethods_SPI
	{
		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS SPI_GetNumChannels(out uint channelCount);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS SPI_GetChannelInfo(uint index, [Out] Ftdi.NativeMethods.FT_DEVICE_LIST_INFO_NODE chanelInfo);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS SPI_OpenChannel(uint index, out IntPtr handle);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS SPI_InitChannel(IntPtr handle, ChannelConfig config);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS SPI_Read(
			IntPtr handle,
			[Out] byte[] buffer, 
			uint sizeToTransfer, 
			out uint sizeTransferred, 
			[MarshalAs(UnmanagedType.U4)] TransferOptions options);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS SPI_Write(
			IntPtr handle,
			[In] byte[] buffer, 
			uint sizeToTransfer, 
			out uint sizeTransferred,
			[MarshalAs(UnmanagedType.U4)] TransferOptions options);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS SPI_ReadWrite(
			IntPtr handle,
			[In] byte[] inBuffer,
			[Out] byte[] outBuffer, 
			uint sizeToTransfer,
			out uint sizeTransferred,
			[MarshalAs(UnmanagedType.U4)] TransferOptions options);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS SPI_IsBusy(
			IntPtr handle,
			out bool state);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS SPI_ChangeCS(
			IntPtr handle,
			[MarshalAs(UnmanagedType.U4)] ConfigOptions configOptions);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS SPI_CloseChannel(IntPtr handle);

		[StructLayout(LayoutKind.Sequential)]
		public class ChannelConfig
		{
			public ChannelConfig(uint clockRate, byte latencyTimer, ConfigOptions options, UInt32 pins)
			{
				this.ClockRate = clockRate;
				this.LatencyTimer = latencyTimer;
				this.Options = options;
				this.Pins = pins;
			}

			public uint ClockRate;

			public byte LatencyTimer;

			[MarshalAs(UnmanagedType.U4)]
			public ConfigOptions Options;

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 Pins;

			[MarshalAs(UnmanagedType.U2)]
			public UInt16 Reserved;
		}

		[Flags]
		public enum ConfigOptions : uint
		{
			None = 0,

			SPI_CONFIG_OPTION_MODE0 = 0,
			SPI_CONFIG_OPTION_MODE1 = 1,
			SPI_CONFIG_OPTION_MODE2 = 2,
			SPI_CONFIG_OPTION_MODE3 = 3,

			SPI_CONFIG_OPTION_CS_DBUS3 = 0,
			SPI_CONFIG_OPTION_CS_DBUS4 = 4,
			SPI_CONFIG_OPTION_CS_DBUS5 = 8,
			SPI_CONFIG_OPTION_CS_DBUS6 = 12,
			SPI_CONFIG_OPTION_CS_DBUS7 = 16,

			SPI_CONFIG_OPTION_CS_ACTIVEHIGH = 0,
			SPI_CONFIG_OPTION_CS_ACTIVELOW = 32,
		}

		[Flags]
		public enum TransferOptions : uint
		{
			None = 0,
			SPI_TRANSFER_OPTIONS_SIZE_IN_BYTES = 0,
			SPI_TRANSFER_OPTIONS_SIZE_IN_BITS = 1,
			SPI_TRANSFER_OPTIONS_CHIPSELECT_ENABLE = 2,
			SPI_TRANSFER_OPTIONS_CHIPSELECT_DISABLE = 4,
		}
	}
}
