using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Protocols.LinearTechnology.LTC6804
{
	public class CellVoltageRegister : RegisterBase
	{
		public CellVoltageRegister()
			: this(new byte[24])
		{
		}

		public CellVoltageRegister(byte[] data)
			: base(data)
		{
		}

		public static CellVoltageRegister FromGroups(byte[] groupA, byte[] groupB, byte[] groupC, byte[] groupD)
		{
			var data = new byte[24];

			if (groupA != null)
				groupA.CopyTo(data, 0);
			if (groupB != null)
				groupB.CopyTo(data, 6);
			if (groupC != null)
				groupC.CopyTo(data, 12);
			if (groupD != null)
				groupD.CopyTo(data, 18);

			return new CellVoltageRegister(data);
		}

		public float this[int cellIndex]
		{
			get { return this.GetCellVoltage(cellIndex); }
		}

		public float GetCellVoltage(int cellIndex)
		{
			Contract.Requires(cellIndex, "cellIndex").ToBeInRange(x => 1 <= x && x <= 12);

			var byteIndex = (cellIndex - 1) * 2;
			var adcValue = this.Data[byteIndex + 1] << 8 | this.Data[byteIndex];
			var voltage = adcValue * 0.0001f;
			return voltage;
		}
	}
}
