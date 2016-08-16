using System;
using System.Collections.Generic;
using System.Linq;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public class BatteryHealthWrapper : DataDictionaryWrapperBase
	{
		public BatteryHealthWrapper(ReadingStorage data)
			: base(data)
		{
		}

		public float FullChargeCapacity
		{
			get { return this.GetValue<float>(FullChargeCapacityKey); }
			set
			{
				Contract.Requires(value, "value").ToBeInRange(x => x >= 0); 
				
				this.SetValue(FullChargeCapacityKey, value);
			}
		}

		public int CycleCount
		{
			get { return this.GetValue<int>(CycleCountKey); }
			set
			{
				Contract.Requires(value, "value").ToBeInRange(x => x >= 0); 
				
				this.SetValue(CycleCountKey, value);
			}
		}

		public float CalculationPrecision
		{
			get { return this.GetValue<float>(CalculationPrecisionKey); }
			set
			{
				Contract.Requires(value, "value").ToBeInRange(x => 0f <= x && x <= 1f); 
				
				this.SetValue(CalculationPrecisionKey, value);
			}
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
