using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor
{
	/// <summary>
	/// Represents design time (constant through battery life-time) information about the battery.
	/// </summary>
	public class BatteryInformation
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BatteryInformation"/> class.
		/// </summary>
		public BatteryInformation()
		{
		}


		/// <summary>
		/// Gets or sets the manufacturer name.
		/// </summary>
		public string Manufacturer { get; set; }

		/// <summary>
		/// Gets or sets the product name.
		/// </summary>
		public string Product { get; set; }

		/// <summary>
		/// Gets or sets the manufacture date.
		/// </summary>
		public DateTime ManufactureDate { get; set; }

		/// <summary>
		/// Gets or sets the serial number.
		/// </summary>
		public int SerialNumber { get; set; }

		/// <summary>
		/// Gets or sets a chemistry of batteries.
		/// </summary>
		public string Chemistry { get; set; }

		/// <summary>
		/// Gets or sets a SMBus specification version.
		/// </summary>
		public Version SpecificationVersion { get; set; }

		/// <summary>
		/// Gets or sets battery cell count.
		/// </summary>
		public int CellCount { get; set; }

		/// <summary>
		/// Gets or sets the battery nominal voltage in volts.
		/// </summary>
		public float NominalVoltage { get; set; }

		//public float DesignedDischargeCurrent { get; set; }
		//public float MaxDischargeCurrent { get; set; }

		/// <summary>
		/// Gets or sets the designed capacity in mAh.
		/// </summary>
		public float DesignedCapacity { get; set; }

		/// <summary>
		/// Gets or sets the scale factor of all voltage values.
		/// </summary>
		public int VoltageScale { get; set; }

		/// <summary>
		/// Gets or sets the scale factor of all current values.
		/// </summary>
		public int CurrentScale { get; set; }

		/// <summary>
		/// Gets the scale factor of all power values.
		/// </summary>
		public int PowerScale
		{
			get { return this.VoltageScale * this.CurrentScale; }
		}
	}
}
