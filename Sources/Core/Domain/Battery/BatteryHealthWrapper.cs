using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public class BatteryHealthWrapper : DataDictionaryWrapperBase, IBatteryHealth
	{
		public BatteryHealthWrapper(ReadingStorage data)
			: base(data)
		{
		}

		public float FullChargeCapacity
		{
			get { return this.GetValue<float>(FullChargeCapacityKey); }
			set { this.SetValue(FullChargeCapacityKey, value); }
		}

		public int CycleCount
		{
			get { return this.GetValue<int>(CycleCountKey); }
			set { this.SetValue(CycleCountKey, value); }
		}

		public float CalculationPrecision
		{
			get { return this.GetValue<float>(CalculationPrecisionKey); }
			set { this.SetValue(CalculationPrecisionKey, value); }
		}


		#region Entry keys

		private const string NamespaceUriName = "BatteryHealthNS";
		private const string FullChargeCapacityEntryName = "FullChargeCapacity";
		private const string CycleCountEntryName = "CycleCount";
		private const string CalculationPrecisionEntryName = "CalculationPrecision";

		public static readonly EntryKey FullChargeCapacityKey = CreateKey(FullChargeCapacityEntryName);
		public static readonly EntryKey CycleCountKey = CreateKey(CycleCountEntryName);
		public static readonly EntryKey CalculationPrecisionKey = CreateKey(CalculationPrecisionEntryName);

		private static EntryKey CreateKey(string entryName)
		{
			return new EntryKey(NamespaceUriName, entryName);
		}

		#endregion Entry keys
	}
}
