using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public class BatteryParametersWrapper : DataDictionaryWrapper, IBatteryParameters
	{
		public const string NamespaceUri = "BatteryParametersNS";
		public const string NominalVoltageEntryName = "NominalVoltage";
		public const string DesignedDischargeCurrentEntryName = "DesignedDischargeCurrent";
		public const string MaxDischargeCurrentEntryName = "MaxDischargeCurrent";
		public const string DesignedCapacityEntryName = "DesignedCapacity";

		public BatteryParametersWrapper(DataDictionary data)
			: base(data)
		{
		}

		protected override string DefaultNamespaceUri 
		{
			get { return NamespaceUri; }
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
	}
}
