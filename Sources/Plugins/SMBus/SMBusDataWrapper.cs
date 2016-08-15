using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Domain.Battery;

namespace ImpruvIT.BatteryMonitor.Protocols.SMBus
{
	public class SMBusDataWrapper : DataDictionaryWrapperBase
	{
		public const string NamespaceUriName = "SMBus";
		public const string SpecificationVersionEntryName = "SpecificationVersion";
		public const string CellCountEntryName = "CellCount";
		public const string VoltageScaleEntryName = "VoltageScale";
		public const string CurrentScaleEntryName = "CurrentScale";

		public SMBusDataWrapper(DataDictionary data)
			: base(data)
		{
		}

		protected override string NamespaceUri
		{
			get { return NamespaceUriName; }
		}


		public Version SpecificationVersion
		{
			get { return this.GetValue<Version>(SpecificationVersionEntryName); }
			set { this.SetValue(SpecificationVersionEntryName, value); }
		}

		public int CellCount
		{
			get { return this.GetValue<int>(CellCountEntryName); }
			set { this.SetValue(CellCountEntryName, value); }
		}

		public int VoltageScale
		{
			get { return this.GetValue<int>(VoltageScaleEntryName); }
			set { this.SetValue(VoltageScaleEntryName, value); }
		}

		public int CurrentScale
		{
			get { return this.GetValue<int>(CurrentScaleEntryName); }
			set { this.SetValue(CurrentScaleEntryName, value); }
		}

		public static EntryKey CreateKey(string entryName)
		{
			return new EntryKey(NamespaceUriName, entryName);
		}
	}
}
