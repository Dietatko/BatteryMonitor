using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Protocols.LinearTechnology.LTC6804
{
	public abstract class RegisterBase
	{
		protected RegisterBase()
			: this(new byte[6])
		{
		}

		protected RegisterBase(byte[] data)
		{
			this.Data = data;
		}

		public byte[] Data { get; private set; }

		protected static bool GetBitValue(byte data, int bit)
		{
			return (data & (1 << bit)) != 0;
		}

		protected static void SetBitValue(ref byte data, int bit, bool value)
		{
			var mask = (byte)(1 << bit);
			if (value)
				data |= mask;
			else
				data &= (byte)~mask;
		}
	}
}
