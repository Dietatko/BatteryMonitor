using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Protocols.LinearTechnology.LTC6804
{
	public static class CommandId
	{
		public const ushort WriteConfigRegister = 0x0001;
		public const ushort ReadConfigRegister = 0x0002;
		public const ushort ReadCellRegisterA = 0x0004;
		public const ushort ReadCellRegisterB = 0x0006;
		public const ushort ReadCellRegisterC = 0x0008;
		public const ushort ReadCellRegisterD = 0x000A;
		public const ushort ReadAuxRegisterA = 0x000C;
		public const ushort ReadAuxRegisterB = 0x000E;
		public const ushort ReadStatusRegisterA = 0x0010;
		public const ushort ReadStatusRegisterB = 0x0012;
		public const ushort ClearCellRegister = 0x0711;
		public const ushort ClearAuxRegister = 0x0712;
		public const ushort ClearStatusRegister = 0x0713;
		public const ushort PollConversionStatus = 0x0714;
		public const ushort DiagnoseMux = 0x0715;
		public const ushort WriteCommRegister = 0x0721;
		public const ushort ReadCommRegister = 0x0722;
		public const ushort StartComm = 0x0723;

		public static ushort StartCellConversion(ConversionMode mode, bool dischargePermitted, int cellIndex)
		{
			Contract.Requires(mode, "mode").ToBeDefinedEnumValue();
			Contract.Requires(cellIndex, "cellIndex").ToBeInRange(x => 0 <= x && x <= 6);

			var commandId = 0x0260;
 			commandId |= (int)mode << 7;
			if (dischargePermitted)
				commandId |= 1 << 4;
			commandId |= cellIndex;

			return (ushort)commandId;
		}

		public static ushort StartOpenWireConversion(ConversionMode mode, bool pullUpCurrent, bool dischargePermitted, int cellIndex)
		{
			Contract.Requires(mode, "mode").ToBeDefinedEnumValue();
			Contract.Requires(cellIndex, "cellIndex").ToBeInRange(x => 0 <= x && x <= 6);

			var commandId = 0x0228;
			commandId |= (int)mode << 7;
			if (pullUpCurrent)
				commandId |= 1 << 6;
			if (dischargePermitted)
				commandId |= 1 << 4;
			commandId |= cellIndex;

			return (ushort)commandId;
		}

		public static ushort StartAuxConversion(ConversionMode mode, int channelIndex)
		{
			Contract.Requires(mode, "mode").ToBeDefinedEnumValue();
			Contract.Requires(channelIndex, "channelIndex").ToBeInRange(x => 0 <= x && x <= 6);

			var commandId = 0x0460;
			commandId |= (int)mode << 7;
			commandId |= channelIndex;

			return (ushort)commandId;
		}

		public static ushort StartCellAuxConversion(ConversionMode mode, bool dischargePermitted)
		{
			Contract.Requires(mode, "mode").ToBeDefinedEnumValue();

			var commandId = 0x046F;
			commandId |= (int)mode << 7;
			if (dischargePermitted)
				commandId |= 1 << 4;

			return (ushort)commandId;
		}

		public static ushort StartStatusConversion(ConversionMode mode, int channelIndex)
		{
			Contract.Requires(mode, "mode").ToBeDefinedEnumValue();
			Contract.Requires(channelIndex, "channelIndex").ToBeInRange(x => 0 <= x && x <= 4);

			var commandId = 0x0468;
			commandId |= (int)mode << 7;
			commandId |= channelIndex;

			return (ushort)commandId;
		}

		public static ushort StartCellSelfTest(ConversionMode conversionMode, SelfTestMode selfTestMode)
		{
			Contract.Requires(conversionMode, "conversionMode").ToBeDefinedEnumValue();
			Contract.Requires(selfTestMode, "selfTestMode").ToBeDefinedEnumValue();

			var commandId = 0x0207;
			commandId |= (int)conversionMode << 7;
			commandId |= (int)selfTestMode << 5;

			return (ushort)commandId;
		}

		public static ushort StartAuxSelfTest(ConversionMode conversionMode, SelfTestMode selfTestMode)
		{
			Contract.Requires(conversionMode, "conversionMode").ToBeDefinedEnumValue();
			Contract.Requires(selfTestMode, "selfTestMode").ToBeDefinedEnumValue();

			var commandId = 0x0407;
			commandId |= (int)conversionMode << 7;
			commandId |= (int)selfTestMode << 5;

			return (ushort)commandId;
		}

		public static ushort StartStatusSelfTest(ConversionMode conversionMode, SelfTestMode selfTestMode)
		{
			Contract.Requires(conversionMode, "conversionMode").ToBeDefinedEnumValue();
			Contract.Requires(selfTestMode, "selfTestMode").ToBeDefinedEnumValue();

			var commandId = 0x040F;
			commandId |= (int)conversionMode << 7;
			commandId |= (int)selfTestMode << 5;

			return (ushort)commandId;
		}
	}

	public enum ConversionMode
	{
		None = 0,
		Fast = 1,
		Normal = 2,
		Filtered = 3
	}

	public enum SelfTestMode
	{
		None = 0,
		Mode1 = 1,
		Mode2 = 2,
	}
}
