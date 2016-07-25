using System;
using System.Collections.Generic;
using System.Linq;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public class Battery : BatteryElement
	{
		public Battery(BatteryElement configuration)
		{
			Contract.Requires(configuration, "configuration").IsNotNull();

			this.Configuration = configuration;
		}

		#region Configuration

		public BatteryElement Configuration { get; private set; }

		public override IProductDefinition Product
		{
			get { return this.Configuration.Product; }
		}

		public override IBatteryParameters ProductionParameters
		{
			get { return this.Configuration.ProductionParameters; }
		}

		public override IBatteryHealth Health
		{
			get { return this.Configuration.Health; }
		}

		public override IBatteryActuals Actuals
		{
			get { return this.Configuration.Actuals; }
		}

		#endregion Configuration
	}
}
