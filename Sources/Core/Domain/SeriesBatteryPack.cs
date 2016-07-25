using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public partial class SeriesBatteryPack : BatteryPack
	{
		private readonly SeriesPackParameters m_params;
		private readonly SeriesPackHealth m_health;
		private readonly SeriesPackActuals m_actuals;

		public SeriesBatteryPack(IEnumerable<BatteryElement> subElements)
			: base(subElements)
		{
			this.m_params = new SeriesPackParameters(this.SubElements);
			this.m_health = new SeriesPackHealth(this.SubElements);
			this.m_actuals = new SeriesPackActuals(this.SubElements);
		}

		public override IBatteryParameters ProductionParameters
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
