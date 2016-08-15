using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public class DesignParametersWrapper : DataDictionaryWrapperBase, IDesignParameters
	{
		public const string NamespaceUriName = "BatteryParametersNS";
		public const string NominalVoltageEntryName = "NominalVoltage";
		public const string DesignedDischargeCurrentEntryName = "DesignedDischargeCurrent";
		public const string MaxDischargeCurrentEntryName = "MaxDischargeCurrent";
		public const string DesignedCapacityEntryName = "DesignedCapacity";

		public DesignParametersWrapper(DataDictionary data)
			: base(data)
		{
		}

		protected override string NamespaceUri 
		{
			get { return NamespaceUriName; }
		}

		public float NominalVoltage
		{
			get { return this.GetValue<float>(NominalVoltageEntryName); }
			set { this.SetValue(NominalVoltageEntryName, value); }
		}

		public float DesignedDischargeCurrent
		{
			get { return this.GetValue<float>(DesignedDischargeCurrentEntryName); }
			set { this.SetValue(DesignedDischargeCurrentEntryName, value); }
		}

		public float MaxDischargeCurrent
		{
			get { return this.GetValue<float>(MaxDischargeCurrentEntryName); }
			set { this.SetValue(MaxDischargeCurrentEntryName, value); }
		}

		public float DesignedCapacity
		{
			get { return this.GetValue<float>(DesignedCapacityEntryName); }
			set { this.SetValue(DesignedCapacityEntryName, value); }
		}

		public static EntryKey CreateKey(string entryName)
		{
			return new EntryKey(NamespaceUriName, entryName);
		}
	}
}
