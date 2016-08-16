using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Domain.Battery;

namespace ImpruvIT.BatteryMonitor.Protocols.LinearTechnology
{
	public class LtDataWrapper : DataDictionaryWrapperBase
	{
		public LtDataWrapper(ReadingStorage data)
			: base(data)
		{
		}


		public int ChipCount
		{
			get { return this.GetValue<int>(ChipCountKey); }
			set { this.SetValue(ChipCountKey, value); }
		}

		public int CellCount
		{
			get { return this.GetValue<int>(CellCountKey); }
			set { this.SetValue(CellCountKey, value); }
		}

		public float SumOfCellVoltages
		{
			get { return this.GetValue<float>(SumOfCellVoltagesKey); }
			set { this.SetValue(SumOfCellVoltagesKey, value); }
		}


		#region Entry keys

		private const string NamespaceUriName = "LT";
		private const string ChipCountEntryName = "ChipCount";
		private const string CellCountEntryName = "CellCount";
		private const string SumOfCellVoltagesEntryName = "SumOfCellVoltages";

		public static readonly EntryKey ChipCountKey = CreateKey(ChipCountEntryName);
		public static readonly EntryKey CellCountKey = CreateKey(CellCountEntryName);
		public static readonly EntryKey SumOfCellVoltagesKey = CreateKey(SumOfCellVoltagesEntryName);

		private static EntryKey CreateKey(string entryName)
		{
			return new EntryKey(NamespaceUriName, entryName);
		}

		#endregion Entry keys
	}

	public static class SMBusDataWrapperExtensions
	{
		public static LtDataWrapper LtData(this BatteryElement batteryElement)
		{
			return new LtDataWrapper(batteryElement.CustomData);
		}
	}
}
