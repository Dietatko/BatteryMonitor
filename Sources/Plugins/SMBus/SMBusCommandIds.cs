using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Protocols.SMBus
{
	public static class SMBusCommandIds
	{
		public static byte ManufacturerAccess = 0x00;
		public static byte RemainingCapacityAlarm = 0x01;
		public static byte RemainingTimeAlarm = 0x02;
		public static byte BatteryMode = 0x03;
		public static byte AtRate = 0x04;
		public static byte AtRateTimeToFull = 0x05;
		public static byte AtRateTimeToEmpty = 0x06;
		public static byte AtRateOK = 0x07;
		public static byte Temperature = 0x08;
		public static byte Voltage = 0x09;
		public static byte Current = 0x0A;
		public static byte AverageCurrent = 0x0B;
		public static byte MaxError = 0x0C;
		public static byte RelativeStateOfCharge = 0x0D;
		public static byte AbsoluteStateOfCharge = 0x0E;
		public static byte RemainingCapacity = 0x0F;
		public static byte FullChargeCapacity = 0x10;
		public static byte RunTimeToEmpty = 0x11;
		public static byte AverageTimeToEmpty = 0x12;
		public static byte AverageTimeToFull = 0x13;
		public static byte ChargingCurrent = 0x14;
		public static byte ChargingVoltage = 0x15;
		public static byte BatteryStatus = 0x16;
		public static byte CycleCount = 0x17;
		public static byte DesignCapacity = 0x18;
		public static byte DesignVoltage = 0x19;
		public static byte SpecificationInfo = 0x1A;
		public static byte ManufactureDate = 0x1B;
		public static byte SerialNumber = 0x1C;
		public static byte ManufacturerName = 0x20;
		public static byte DeviceName = 0x21;
		public static byte DeviceChemistry = 0x22;
		public static byte CellCount = 0x2F;
		public static byte CellVoltage4 = 0x3C;
		public static byte CellVoltage3 = 0x3D;
		public static byte CellVoltage2 = 0x3E;
		public static byte CellVoltage1 = 0x3F;
	}
}
