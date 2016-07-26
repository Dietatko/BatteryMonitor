using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Protocols.SMBus
{
	public static class SMBusCommandIds
	{
		public static uint ManufacturerAccess = 0x00;
		public static uint RemainingCapacityAlarm = 0x01;
		public static uint RemainingTimeAlarm = 0x02;
		public static uint BatteryMode = 0x03;
		public static uint AtRate = 0x04;
		public static uint AtRateTimeToFull = 0x05;
		public static uint AtRateTimeToEmpty = 0x06;
		public static uint AtRateOK = 0x07;
		public static uint Temperature = 0x08;
		public static uint Voltage = 0x09;
		public static uint Current = 0x0A;
		public static uint AverageCurrent = 0x0B;
		public static uint MaxError = 0x0C;
		public static uint RelativeStateOfCharge = 0x0D;
		public static uint AbsoluteStateOfCharge = 0x0E;
		public static uint RemainingCapacity = 0x0F;
		public static uint FullChargeCapacity = 0x10;
		public static uint RunTimeToEmpty = 0x11;
		public static uint AverageTimeToEmpty = 0x12;
		public static uint AverageTimeToFull = 0x13;
		public static uint ChargingVoltage = 0x14;
		public static uint ChargingCurrent = 0x15;
		public static uint BatteryStatus = 0x16;
		public static uint CycleCount = 0x17;
		public static uint DesignCapacity = 0x18;
		public static uint DesignVoltage = 0x19;
		public static uint SpecificationInfo = 0x1A;
		public static uint ManufactureDate = 0x1B;
		public static uint SerialNumber = 0x1C;
		public static uint ManufacturerName = 0x20;
		public static uint DeviceName = 0x21;
		public static uint DeviceChemistry = 0x22;
		public static uint CellCount = 0x2F;
		public static uint CellVoltage4 = 0x3C;
		public static uint CellVoltage3 = 0x3D;
		public static uint CellVoltage2 = 0x3E;
		public static uint CellVoltage1 = 0x3F;
	}
}
