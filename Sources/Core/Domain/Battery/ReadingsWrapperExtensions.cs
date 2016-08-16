using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public static class ReadingsWrapperExtensions
	{
		public static ProductDefinitionWrapper ProductDefinition(this BatteryElement batteryElement)
		{
			return new ProductDefinitionWrapper(batteryElement.CustomData);
		}

		public static DesignParametersWrapper DesignParameters(this BatteryElement batteryElement)
		{
			return new DesignParametersWrapper(batteryElement.CustomData);
		}

		public static BatteryHealthWrapper Health(this BatteryElement batteryElement)
		{
			return new BatteryHealthWrapper(batteryElement.CustomData);
		}

		public static BatteryActualsWrapper Actuals(this BatteryElement batteryElement)
		{
			return new BatteryActualsWrapper(batteryElement.CustomData);
		}
	}
}
