using System;
using System.Collections.Generic;
using System.Linq;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public class DesignParametersWrapper : DataDictionaryWrapperBase
	{
		public DesignParametersWrapper(ReadingStorage data)
			: base(data)
		{
		}


		public float NominalVoltage
		{
			get { return this.GetValue<float>(NominalVoltageKey); }
			set
			{
				Contract.Requires(value, "value").ToBeInRange(x => x >= 0); 
				
				this.SetValue(NominalVoltageKey, value);
			}
		}

		public float DesignedDischargeCurrent
		{
			get { return this.GetValue<float>(DesignedDischargeCurrentKey); }
			set
			{
				Contract.Requires(value, "value").ToBeInRange(x => x >= 0); 
				
				this.SetValue(DesignedDischargeCurrentKey, value);
			}
		}

		public float MaxDischargeCurrent
		{
			get { return this.GetValue<float>(MaxDischargeCurrentKey); }
			set
			{
				Contract.Requires(value, "value").ToBeInRange(x => x >= 0);

				this.SetValue(MaxDischargeCurrentKey, value);
			}
		}

		public float DesignedCapacity
		{
			get { return this.GetValue<float>(DesignedCapacityKey); }
			set
			{
				Contract.Requires(value, "value").ToBeInRange(x => x >= 0); 
				
				this.SetValue(DesignedCapacityKey, value);
			}
		}


		#region Entry keys

		private const string NamespaceUriName = "BatteryParametersNS";
		private const string NominalVoltageEntryName = "NominalVoltage";
		private const string DesignedDischargeCurrentEntryName = "DesignedDischargeCurrent";
		private const string MaxDischargeCurrentEntryName = "MaxDischargeCurrent";
		private const string DesignedCapacityEntryName = "DesignedCapacity";

		public static readonly EntryKey NominalVoltageKey = CreateKey(NominalVoltageEntryName);
		public static readonly EntryKey DesignedDischargeCurrentKey = CreateKey(DesignedDischargeCurrentEntryName);
		public static readonly EntryKey MaxDischargeCurrentKey = CreateKey(MaxDischargeCurrentEntryName);
		public static readonly EntryKey DesignedCapacityKey = CreateKey(DesignedCapacityEntryName);

		private static EntryKey CreateKey(string entryName)
		{
			return new EntryKey(NamespaceUriName, entryName);
		}

		#endregion Entry keys
	}
}
