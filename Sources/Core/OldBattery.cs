using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor
{
    public class OldBattery
    {
        public OldBattery()
        {
			this.Information = new BatteryInformation();
			this.Status = new BatteryStatus();
			this.Conditions = new BatteryConditions();
        }

		public BatteryInformation Information { get; private set; }
		public BatteryStatus Status { get; private set; }
		public BatteryConditions Conditions { get; private set; }
    }
}
