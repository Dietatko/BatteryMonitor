using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

using FTD2XX_NET;

namespace ImpruvIT.BatteryMonitor.Hardware.Ftdi
{
	public static class NativeMethods
	{
		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Init_libMPSSE();

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Cleanup_libMPSSE();

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS FT_ReadGPIO(IntPtr handle, out byte value);

		[DllImport("libMPSSE.dll", SetLastError = false, CallingConvention = CallingConvention.Cdecl)]
		public static extern FTDI.FT_STATUS FT_WriteGPIO(IntPtr handle, byte dir, byte value);
		
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public class FT_DEVICE_LIST_INFO_NODE
		{
			public FT_FLAGS Flags;

			[MarshalAs(UnmanagedType.U4)]
			public FTDI.FT_DEVICE Type;

			public uint ID;

			public uint LocId;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
			public string SerialNumber;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
			public string Description;

			public IntPtr ftHandle;
		}

		[Flags]
		public enum FT_FLAGS : uint
		{
			None = 0,
			FT_FLAGS_OPENED = 1,
			FT_FLAGS_HISPEED = 2
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
