using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.WPFApp.ViewLogic
{
	public class ConditionsRecord
	{
		public ConditionsRecord(BatteryConditions conditions)
			: this(DateTime.UtcNow, conditions)
		{
		}

		public ConditionsRecord(DateTime timestamp, BatteryConditions conditions)
		{
			this.Timestamp = timestamp;
			this.Conditions = conditions;
		}

		public DateTime Timestamp { get; private set; }
		public BatteryConditions Conditions { get; private set; }
	}
}
