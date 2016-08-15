using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public class BatteryHealthWrapper : DataDictionaryWrapperBase, IBatteryHealth
	{
		public const string NamespaceUriName = "BatteryHealthNS";
		public const string FullChargeCapacityEntryName = "FullChargeCapacity";
		public const string CycleCountEntryName = "CycleCount";
		public const string CalculationPrecisionEntryName = "CalculationPrecision";

		public BatteryHealthWrapper(DataDictionary data)
			: base(data)
		{
		}

		protected override string NamespaceUri 
		{
			get { return NamespaceUriName; }
		}

		public float FullChargeCapacity
		{
			get { return this.GetValue<float>(FullChargeCapacityEntryName); }
			set { this.SetValue(FullChargeCapacityEntryName, value); }
		}

		public int CycleCount
		{
			get { return this.GetValue<int>(CycleCountEntryName); }
			set { this.SetValue(CycleCountEntryName, value); }
		}

		public float CalculationPrecision
		{
			get { return this.GetValue<float>(CalculationPrecisionEntryName); }
			set { this.SetValue(CalculationPrecisionEntryName, value); }
		}

		public static EntryKey CreateKey(string entryName)
		{
			return new EntryKey(NamespaceUriName, entryName);
		}
	}
}
