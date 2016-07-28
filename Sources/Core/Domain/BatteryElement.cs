using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain
{
	public abstract class BatteryElement
	{
		protected BatteryElement()
		{
			this.CustomData = new DataDictionary();
			this.CustomData.ValueChanged += (s, a) => this.OnValueChanged(a);
		}

		public DataDictionary CustomData { get; private set; }

		public abstract IProductDefinition Product { get; }
		public abstract IDesignParameters DesignParameters { get; }
		public abstract IBatteryHealth Health { get; }
		public abstract IBatteryActuals Actuals { get; }

		//#region Commands

		//public abstract float RemainingCapacityAlarm { get; }
		//public abstract float RemainingTimeAlarm { get; }
		//public abstract float ChargingVoltage { get; }		// ???
		//public abstract float ChargingCurrent { get; }		// ???

		//#endregion Commands

		public event EventHandler<EntryKey> ValueChanged;

		protected virtual void OnValueChanged(EntryKey key)
		{
			var handlers = this.ValueChanged;
			if (handlers != null)
				handlers(this, key);
		}
	}
}
