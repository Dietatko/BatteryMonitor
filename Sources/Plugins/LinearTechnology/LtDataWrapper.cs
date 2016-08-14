using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain;

namespace ImpruvIT.BatteryMonitor.Protocols.LinearTechnology
{
	public class LtDataWrapper : DataDictionaryWrapperBase
	{
		public const string NamespaceUriName = "LT";
		public const string ChipCountEntryName = "ChipCount";
		public const string CellCountEntryName = "CellCount";

		public LtDataWrapper(DataDictionary data)
			: base(data)
		{
		}

		protected override string NamespaceUri
		{
			get { return NamespaceUriName; }
		}


		public int CellCount
		{
			get { return this.GetValue<int>(CellCountEntryName); }
			set { this.SetValue(CellCountEntryName, value); }
		}

		public int ChipCount
		{
			get { return this.GetValue<int>(ChipCountEntryName); }
			set { this.SetValue(ChipCountEntryName, value); }
		}

		public static EntryKey CreateKey(string entryName)
		{
			return new EntryKey(NamespaceUriName, entryName);
		}
	}
}
