using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Protocols.LinearTechnology.LTC6804
{
	public class StatusRegister : RegisterBase
	{
		public StatusRegister()
			: this(new byte[12])
		{
		}

		public StatusRegister(byte[] data)
			: base(data)
		{
		}

		public static StatusRegister FromGroups(byte[] groupA, byte[] groupB)
		{
			var data = new byte[12];

			if (groupA != null)
				groupA.CopyTo(data, 0);
			if (groupB != null)
				groupB.CopyTo(data, 6);

			return new StatusRegister(data);
		}

		public float PackVoltage
		{
			get { return this.GetVoltage(0) * 20; }
		}

		public float DieTemperature
		{
			get { return this.GetVoltage(2) / 0.0075f; }
		}

		public float AnalogSupplyVoltage
		{
			get { return this.GetVoltage(4); }
		}

		public float DigitalSupplyVoltage
		{
			get { return this.GetVoltage(6); }
		}

		public bool MuxFail
		{
			get { return GetBitValue(this.Data[11], 1); }
		}

		public bool ThermalShutdownOccurred
		{
			get { return GetBitValue(this.Data[11], 0); }
		}

		private float GetVoltage(int byteOffset)
		{
			Contract.Requires(byteOffset, "byteOffset").ToBeInRange(x => 0 <= x && x <= 6);

			var adcValue = this.Data[byteOffset + 1] << 8 | this.Data[byteOffset];
			var voltage = adcValue * 0.0001f;
			return voltage;
		}
	}
}
