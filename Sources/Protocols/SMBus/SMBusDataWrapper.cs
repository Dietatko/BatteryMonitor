using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain;

namespace ImpruvIT.BatteryMonitor.Protocols.SMBus
{
	public class SMBusDataWrapper : DataDictionaryWrapper
	{
		public const string NamespaceUri = "SMBus";
		public const string SpecificationVersionEntryName = "SpecificationVersion";
		public const string VoltageScaleEntryName = "VoltageScale";
		public const string CurrentScaleEntryName = "CurrentScale";

		public SMBusDataWrapper(DataDictionary data)
			: base(data)
		{
		}

		protected override string DefaultNamespaceUri
		{
			get { return NamespaceUri; }
		}


		public Version SpecificationVersion
		{
			get { return this.GetValue<Version>(SpecificationVersionEntryName); }
			set { this.SetValue(SpecificationVersionEntryName, value); }
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
	}
}
