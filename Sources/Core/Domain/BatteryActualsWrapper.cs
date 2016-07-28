using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public class BatteryActualsWrapper : DataDictionaryWrapperBase, IBatteryActuals
	{
		public const string NamespaceUriName = "BatteryReadingsNS";

		public const string VoltageEntryName = "Voltage";
		public const string ActualCurrentEntryName = "ActualCurrent";
		public const string AverageCurrentEntryName = "AverageCurrent";
		public const string TemperatureEntryName = "Temperature";

		public const string RemainingCapacityEntryName = "RemainingCapacity";
		public const string AbsoluteStateOfChargeEntryName = "AbsoluteStateOfCharge";
		public const string RelativeStateOfChargeEntryName = "RelativeStateOfCharge";

		public const string ActualRunTimeEntryName = "ActualRunTime";
		public const string AverageRunTimeEntryName = "AverageRunTime";

		public BatteryActualsWrapper(DataDictionary data)
			: base(data)
		{
		}

		protected override string NamespaceUri 
		{
			get { return NamespaceUriName; }
		}


		#region Readings

		public float Voltage
		{
			get { return this.GetValue<float>(VoltageEntryName); }
			set { this.SetValue(VoltageEntryName, value); }
		}

		public float ActualCurrent
		{
			get { return this.GetValue<float>(ActualCurrentEntryName); }
			set { this.SetValue(ActualCurrentEntryName, value); }
		}

		public float AverageCurrent
		{
			get { return this.GetValue<float>(AverageCurrentEntryName); }
			set { this.SetValue(AverageCurrentEntryName, value); }
		}

		public float Temperature
		{
			get { return this.GetValue<float>(TemperatureEntryName); }
			set { this.SetValue(TemperatureEntryName, value); }
		}

		#endregion Readings


		#region State of charge
		
		public float RemainingCapacity
		{
			get { return this.GetValue<float>(RemainingCapacityEntryName); }
			set { this.SetValue(RemainingCapacityEntryName, value); }
		}

		public float AbsoluteStateOfCharge
		{
			get { return this.GetValue<float>(AbsoluteStateOfChargeEntryName); }
			set { this.SetValue(AbsoluteStateOfChargeEntryName, value); }
		}

		public float RelativeStateOfCharge
		{
			get { return this.GetValue<float>(RelativeStateOfChargeEntryName); }
			set { this.SetValue(RelativeStateOfChargeEntryName, value); }
		}

		#endregion State of charge


		#region Run time estimations

		public TimeSpan ActualRunTime
		{
			get { return this.GetValue<TimeSpan>(ActualRunTimeEntryName); }
			set { this.SetValue(ActualRunTimeEntryName, value); }
		}

		public TimeSpan AverageRunTime
		{
			get { return this.GetValue<TimeSpan>(AverageRunTimeEntryName); }
			set { this.SetValue(AverageRunTimeEntryName, value); }
		}

		#endregion Run time estimations

		public static EntryKey CreateKey(string entryName)
		{
			return new EntryKey(NamespaceUriName, entryName);
		}
	}
}
