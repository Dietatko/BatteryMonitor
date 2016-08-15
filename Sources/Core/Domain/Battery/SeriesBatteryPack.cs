using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public partial class SeriesBatteryPack : BatteryPack
	{
		private readonly SeriesPackDesignParameters m_params;
		private readonly SeriesPackHealth m_health;
		private readonly SeriesPackActuals m_actuals;

		public SeriesBatteryPack(IEnumerable<BatteryElement> subElements)
			: base(subElements)
		{
			this.m_params = new SeriesPackDesignParameters(this.SubElements);
			this.m_health = new SeriesPackHealth(this.SubElements);
			this.m_actuals = new SeriesPackActuals(this.SubElements);
		}

		public override IDesignParameters DesignParameters
		{
			get { return this.m_params; }
		}

		public override IBatteryHealth Health
		{
			get { return this.m_health; }
		}

		public override IBatteryActuals Actuals
		{
			get { return this.m_actuals; }
		}
	}
}
