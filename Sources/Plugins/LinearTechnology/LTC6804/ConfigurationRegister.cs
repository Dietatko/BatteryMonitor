using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Protocols.LinearTechnology.LTC6804
{
	public class ConfigurationRegister : RegisterBase
	{
		public ConfigurationRegister()
			: this(new byte[6])
		{
		}

		public ConfigurationRegister(byte[] data)
			: base(data)
		{
		}

		public bool Gpio1PullDown
		{
			get { return this.GetGpioPullDown(0); }
			set { this.SetGpioPullDown(0, value); }
		}

		public bool Gpio2PullDown
		{
			get { return this.GetGpioPullDown(1); }
			set { this.SetGpioPullDown(1, value); }
		}

		public bool Gpio3PullDown
		{
			get { return this.GetGpioPullDown(2); }
			set { this.SetGpioPullDown(2, value); }
		}

		public bool Gpio4PullDown
		{
			get { return this.GetGpioPullDown(3); }
			set { this.SetGpioPullDown(3, value); }
		}

		public bool Gpio5PullDown
		{
			get { return this.GetGpioPullDown(4); }
			set { this.SetGpioPullDown(4, value); }
		}

		public bool ReferenceOn
		{
			get { return GetBitValue(this.Data[0], 2); }
			set { SetBitValue(ref this.Data[0], 2, value); }
		}

		public bool SwTimerOn
		{
			get { return GetBitValue(this.Data[0], 1); }
		}

		public bool AdcMode
		{
			get { return GetBitValue(this.Data[0], 0); }
			set { SetBitValue(ref this.Data[0], 0, value); }
		}

		public float UnderVoltage
		{
			get
			{
				var adcValue = ((this.Data[2] & 0x0F) << 8) | this.Data[1];
				var voltage = (adcValue + 1) * 16 * 0.0001f;
				return voltage;
			}
			set
			{
				Contract.Requires(value, "value").ToBeInRange(x => 0 <= x && x <= 5.0f);

				var adcValue = (int)((value / 0.0001f) / 16) - 1;
				adcValue = Math.Max(adcValue, 0);

				this.Data[2] &= 0xF0;
				this.Data[2] |= (byte)((adcValue >> 8) & 0x0F);
				this.Data[1] = (byte)adcValue;
			}
		}

		public float OverVoltage
		{
			get
			{
				var adcValue = (this.Data[3] << 4) | (this.Data[2] >> 4);
				var voltage = adcValue * 16 * 0.0001f;
				return voltage;
			}
			set
			{
				Contract.Requires(value, "value").ToBeInRange(x => 0 <= x && x <= 5.0f);

				var adcValue = (int)((value / 0.0001f) / 16);

				this.Data[2] &= 0x0F;
				this.Data[2] |= (byte)(adcValue << 4);
				this.Data[3] = (byte)(adcValue >> 4);
			}
		}

		public bool GetGpioPullDown(int gpio)
		{
			Contract.Requires(gpio, "gpio").ToBeInRange(x => 0 <= x && x <= 4);

			return !GetBitValue(this.Data[0], gpio + 3);
		}

		public void SetGpioPullDowns(bool value)
		{
			for (int i = 0; i < 5; i++)
				this.SetGpioPullDown(i, value);
		}

		public void SetGpioPullDown(int gpio, bool value)
		{
			Contract.Requires(gpio, "gpio").ToBeInRange(x => 0 <= x && x <= 4);

			SetBitValue(ref this.Data[0], gpio + 3, !value); 
		}

		public bool GetDischargeSwitch(int cellIndex)
		{
			Contract.Requires(cellIndex, "cellIndex").ToBeInRange(x => 0 <= x && x <= 11);

			int byteIndex;
			int bitIndex;
			if (cellIndex <= 7)
			{
				byteIndex = 4;
				bitIndex = cellIndex;
			}
			else
			{
				byteIndex = 5;
				bitIndex = cellIndex - 8;
			}

			return GetBitValue(this.Data[byteIndex], bitIndex);
		}

		public void SetDischargeSwitch(int cellIndex, bool enableDischarge)
		{
			Contract.Requires(cellIndex, "cellIndex").ToBeInRange(x => 0 <= x && x <= 11);

			int byteIndex;
			int bitIndex;
			if (cellIndex <= 7)
			{
				byteIndex = 4;
				bitIndex = cellIndex;
			}
			else
			{
				byteIndex = 5;
				bitIndex = cellIndex - 8;
			}

			SetBitValue(ref this.Data[byteIndex], bitIndex, enableDischarge);
		}

		public DischargeTime GetDischargeTimeout()
		{
			var value = (DischargeTime)(this.Data[5] >> 4);
			return value;
		}

		public void SetDischargeTimeout(DischargeTime dischargeTime)
		{
			Contract.Requires(dischargeTime, "dischargeTime").ToBeDefinedEnumValue();

			this.Data[5] &= 0x0F;
			this.Data[5] |= (byte)((int)dischargeTime << 4);
		}
	}

	public enum DischargeTime
	{
		Disabled = 0x0,
		Sec30 = 0x1,
		Min1 = 0x2,
		Min2 = 0x3,
		Min3 = 0x4,
		Min4 = 0x5,
		Min5 = 0x6,
		Min10 = 0x7,
		Min15 = 0x8,
		Min20 = 0x9,
		Min30 = 0xA,
		Min40 = 0xB,
		Min60 = 0xC,
		Min75 = 0xD,
		Min90 = 0xE,
		Min120 = 0xF,
	}
}
