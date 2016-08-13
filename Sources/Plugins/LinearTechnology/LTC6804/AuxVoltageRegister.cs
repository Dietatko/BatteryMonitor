using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Protocols.LinearTechnology.LTC6804
{
	public class AuxVoltageRegister : RegisterBase
	{
		public AuxVoltageRegister()
			: this(new byte[12])
		{
		}

		public AuxVoltageRegister(byte[] data)
			: base(data)
		{
		}

		public static AuxVoltageRegister FromGroups(byte[] groupA, byte[] groupB)
		{
			var data = new byte[12];

			if (groupA != null)
				groupA.CopyTo(data, 0);
			if (groupB != null)
				groupB.CopyTo(data, 6);

			return new AuxVoltageRegister(data);
		}

		public float this[int gpioIndex]
		{
			get { return this.GetAuxVoltage(gpioIndex); }
		}

		public float Ref2Voltage
		{
			get { return this.ReadVoltage(5); }
		}

		public float GetAuxVoltage(int gpioIndex)
		{
			Contract.Requires(gpioIndex, "gpioIndex").ToBeInRange(x => 1 <= x && x <= 5);

			return this.ReadVoltage(gpioIndex - 1);
		}

		private float ReadVoltage(int index)
		{
			Contract.Requires(index, "gpioIndex").ToBeInRange(x => 0 <= x && x <= 5);

			var byteIndex = index * 2;
			var adcValue = this.Data[byteIndex + 1] << 8 | this.Data[byteIndex];
			var voltage = adcValue * 0.0001f;
			return voltage;
		}
	}
}
