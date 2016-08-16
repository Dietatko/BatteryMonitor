using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Domain.Battery;

namespace ImpruvIT.BatteryMonitor.Protocols.LinearTechnology.LTC6804
{
	public class SingleCell : Cell
	{
		public SingleCell()
		{
			this.InitializeReadings();
		}

		protected void InitializeReadings()
		{
			this.CustomData.CreateValue(BatteryActualsWrapper.VoltageKey, new TypedReadingValue<float>());
		}
	}
}
