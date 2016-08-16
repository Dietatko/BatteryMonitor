using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Domain.Battery;

namespace ImpruvIT.BatteryMonitor.Protocols.SMBus
{
	public class SingleCell : Cell
	{
		public SingleCell(float nominalVoltage)
		{
			this.InitializeReadings();

			this.DesignParameters().NominalVoltage = nominalVoltage;
		}

		protected void InitializeReadings()
		{
			this.CustomData.CreateValue(new TypedReadingValue<float>(DesignParametersWrapper.NominalVoltageKey));
			this.CustomData.CreateValue(new TypedReadingValue<float>(BatteryActualsWrapper.VoltageKey));
		}
	}
}
