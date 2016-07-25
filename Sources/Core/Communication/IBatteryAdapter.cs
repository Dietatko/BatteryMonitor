using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ImpruvIT.BatteryMonitor.Communication
{
	public interface IBatteryAdapter
	{
		OldBattery Battery { get; }
		bool IsMonitoringConditions { get; }
		void RecognizeBattery();

		/// <summary>
		/// Setup battery to report alarm when remaining capacity is lower than set values.
		/// </summary>
		/// <param name="remainingCapacity">Remaining capacity treshold (in Ah); <i>0</i> to disable the alarm; <i>null</i> not to change the threshold.</param>
		/// <param name="remainingTime">Remaining time treshold; <i>zero timespan</i> to disable the alarm; <i>null</i> not to change the threshold.</param>
		void SetupRemainingCapacityAlarm(float? remainingCapacity, TimeSpan? remainingTime);

		void ReadStatus();
		void StartMonitoringConditions(TimeSpan updateInterval);
		void StopMonitoringConditions();
		void ReadActualConditions(params Expression<Func<BatteryConditions, object>>[] valueSelectors);
		void ReadActualConditions(IEnumerable<Expression<Func<BatteryConditions, object>>> valueSelectors = null);
		event EventHandler<CurrentConditionsEventArgs> CurrentConditionsUpdated;
	}
}
