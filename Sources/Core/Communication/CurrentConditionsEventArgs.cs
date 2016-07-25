using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Communication
{
    public class CurrentConditionsEventArgs : EventArgs
    {
        public CurrentConditionsEventArgs(BatteryConditions conditions)
        {
            this.Conditions = conditions;
        }

		public BatteryConditions Conditions { get; private set; }
    }
}
