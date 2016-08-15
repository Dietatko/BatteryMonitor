using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Domain.Battery;

namespace ImpruvIT.BatteryMonitor.Protocols.SMBus
{
	public class SMBusDataWrapper : DataDictionaryWrapperBase
	{
		public SMBusDataWrapper(ReadingStorage data)
			: base(data)
		{
		}

		
		public Version SpecificationVersion
		{
			get { return this.GetValue<Version>(SpecificationVersionKey); }
			set { this.SetValue(SpecificationVersionKey, value); }
		}

		public int CellCount
		{
			get { return this.GetValue<int>(CellCountKey); }
			set { this.SetValue(CellCountKey, value); }
		}

		public int VoltageScale
		{
			get { return this.GetValue<int>(VoltageScaleKey); }
			set { this.SetValue(VoltageScaleKey, value); }
		}

		public int CurrentScale
		{
			get { return this.GetValue<int>(CurrentScaleKey); }
			set { this.SetValue(CurrentScaleKey, value); }
		}


		#region Entry keys

		private const string NamespaceUriName = "SMBus";
		private const string SpecificationVersionEntryName = "SpecificationVersion";
		private const string CellCountEntryName = "CellCount";
		private const string VoltageScaleEntryName = "VoltageScale";
		private const string CurrentScaleEntryName = "CurrentScale";

		public static readonly EntryKey SpecificationVersionKey = CreateKey(SpecificationVersionEntryName);
		public static readonly EntryKey CellCountKey = CreateKey(CellCountEntryName);
		public static readonly EntryKey VoltageScaleKey = CreateKey(VoltageScaleEntryName);
		public static readonly EntryKey CurrentScaleKey = CreateKey(CurrentScaleEntryName);

		private static EntryKey CreateKey(string entryName)
		{
			return new EntryKey(NamespaceUriName, entryName);
		}

		#endregion Entry keys
	}
}
