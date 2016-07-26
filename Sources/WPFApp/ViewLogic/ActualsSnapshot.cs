using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain;

namespace ImpruvIT.BatteryMonitor.WPFApp.ViewLogic
{
	public class ActualsSnapshot
	{
		public ActualsSnapshot(Actuals actuals)
			: this(DateTime.UtcNow, actuals)
		{
		}

		public ActualsSnapshot(DateTime timestamp, Actuals actuals)
		{
			this.Timestamp = timestamp;
			this.Actuals = actuals;
		}

		public DateTime Timestamp { get; private set; }
		public Actuals Actuals { get; private set; }
	}

	public class Actuals
	{
		public float PackVoltage { get; set; }
		public float Cell1Voltage { get; set; }
		public float Cell2Voltage { get; set; }
		public float Cell3Voltage { get; set; }
		public float ActualCurrent { get; set; }
		public float Capacity { get; set; }
		public float Temperature { get; set; }
	}
}
