using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor
{
	/// <summary>
	/// Represents long-term status of the battery.
	/// </summary>
	public class BatteryStatus
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BatteryStatus"/> class.
		/// </summary>
		public BatteryStatus()
		{
		}

		// Deprecation of battery status
		/// <summary>
		/// Gets or sets the full charged capacity in Ah.
		/// </summary>
		public float FullChargeCapacity { get; set; }

		/// <summary>
		/// Gets or sets the number of cycles in battery lifetime.
		/// </summary>
		public int CycleCount { get; set; }

		/// <summary>
		/// Gets or sets the maximal error of calculated values (precision of reported values).
		/// </summary>
		public int MaxError { get; set; }

		// Settings
		/// <summary>
		/// Gets or sets the remaining capacity treshold (in Ah) when battery reports alarm notification to a host.
		/// </summary>
		public float RemainingCapacityAlarm { get; set; }

		/// <summary>
		/// Get or sets the remaining time left when battery reports alarm notification to a host.
		/// </summary>
		public TimeSpan RemainingTimeAlarm { get; set; }

		//public int BatteryMode { get; set; }
		//public int BatteryStatus { get; set; }
	}
}
