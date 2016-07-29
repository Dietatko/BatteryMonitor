using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain.Description
{
	public static class ReadingDescriptors
	{
		#region Product

		public static readonly ReadingDescriptor Manufacturer = new ReadingDescriptor(
			new ReadingDescription(
				"Manufacturer",
				"The manufacturer of the battery pack."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, ProductDefinitionWrapper.CreateKey(ProductDefinitionWrapper.ManufacturerEntryName) }
			},
			new ReadingValueAccessor(
				b => b.Product.Manufacturer
			)
			);

		public static readonly ReadingDescriptor Product = new ReadingDescriptor(
			new ReadingDescription(
				"Product", 
				"The battery pack product name."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, ProductDefinitionWrapper.CreateKey(ProductDefinitionWrapper.ProductEntryName) }
			},
			new ReadingValueAccessor(
				b => b.Product.Product
			));

		public static readonly ReadingDescriptor ManufactureDate = new ReadingDescriptor(
			new ReadingDescription(
				"Manufacture date",
				"The battery pack manufacture date."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, ProductDefinitionWrapper.CreateKey(ProductDefinitionWrapper.ManufactureDateEntryName) }
			},
			new ReadingValueAccessor(
				b => b.Product.ManufactureDate,
				"{0:d}"
			));

		public static readonly ReadingDescriptor SerialNumber = new ReadingDescriptor(
			new ReadingDescription(
				"Serial number", 
				"The battery pack serial number."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, ProductDefinitionWrapper.CreateKey(ProductDefinitionWrapper.SerialNumberEntryName) }
			},
			new ReadingValueAccessor(
				b => b.Product.SerialNumber
			));

		public static readonly ReadingDescriptor Chemistry = new ReadingDescriptor(
			new ReadingDescription(
				"Chemistry", 
				"The battery pack chemistry."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, ProductDefinitionWrapper.CreateKey(ProductDefinitionWrapper.ChemistryEntryName) }
			},
			new ReadingValueAccessor(
				b => b.Product.Chemistry
			));

		#endregion Product

		#region Design parameters

		public static readonly ReadingDescriptor NominalVoltage = new ChartReadingDescriptor(
			new ReadingDescription(
				"Nominal voltage", 
				"The nominal voltage of the battery pack."
			),
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, DesignParametersWrapper.CreateKey(DesignParametersWrapper.NominalVoltageEntryName) }
			},
			new ReadingValueAccessor(
				b => b.DesignParameters.NominalVoltage,
				"{0} V"
			),
			new ReadingVisualizer(
				x => (double)x
			));

		public static readonly ReadingDescriptor DesignedDischargeCurrent = new ChartReadingDescriptor(
			new ReadingDescription(
				"Discharge current", 
				"A continuos discharge current of the battery pack."
			),
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, DesignParametersWrapper.CreateKey(DesignParametersWrapper.DesignedCapacityEntryName) }
			},
			new ReadingValueAccessor(
				b => b.DesignParameters.DesignedDischargeCurrent,
				"{0} A"
			),
			new ReadingVisualizer(
				x => (double)x
			));

		public static readonly ReadingDescriptor MaxDischargeCurrent = new ChartReadingDescriptor(
			new ReadingDescription(
				"Max discharge current", 
				"A maximal short-time (pulse) discharge current of the battery pack."
			),
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, DesignParametersWrapper.CreateKey(DesignParametersWrapper.MaxDischargeCurrentEntryName) }
			},
			new ReadingValueAccessor(
				b => b.DesignParameters.MaxDischargeCurrent,
				"{0} A"
			),
			new ReadingVisualizer(
				x => (double)x
			));

		public static readonly ReadingDescriptor DesignedCapacity = new ChartReadingDescriptor(
			new ReadingDescription(
				"Nominal capacity", 
				"A designed capacity of the battery pack."
			),
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, DesignParametersWrapper.CreateKey(DesignParametersWrapper.DesignedCapacityEntryName) }
			},
			new ReadingValueAccessor(
				b => b.DesignParameters.DesignedCapacity * 1000,
				"{0} mAh"
			),
			new ReadingVisualizer(
				x => (double)x
			));

		#endregion Design parameters

		#region Health

		public static readonly ReadingDescriptor FullChargeCapacity = new ChartReadingDescriptor(
			new ReadingDescription(
				"Full charge capacity", 
				"A capacity of the full-charged battery pack."
			),
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, BatteryHealthWrapper.CreateKey(BatteryHealthWrapper.FullChargeCapacityEntryName) }
			},
			new ReadingValueAccessor(
				b => b.Health.FullChargeCapacity * 1000,
				"{0} mAh"
			),
			new ReadingVisualizer(
				x => (double)x
			));

		public static readonly ReadingDescriptor CycleCount = new ChartReadingDescriptor(
			new ReadingDescription(
				"Cycles", 
				"A number of charge-discharge cycles in life time of the battery pack."
			),
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, BatteryHealthWrapper.CreateKey(BatteryHealthWrapper.CycleCountEntryName) }
			},
			new ReadingValueAccessor(
				b => b.Health.CycleCount,
				"{0} cycles"
			),
			new ReadingVisualizer(
				x => (double)x
			));

		public static readonly ReadingDescriptor CalculationPrecision = new ChartReadingDescriptor(
			new ReadingDescription(
				"Calculation precision", 
				"A maximum value error of measured and calculated values."
			),
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, BatteryHealthWrapper.CreateKey(BatteryHealthWrapper.CalculationPrecisionEntryName) }
			},
			new ReadingValueAccessor(
				b => b.Health.CalculationPrecision * 100,
				"{0} %"
			),
			new ReadingVisualizer(
				x => (double)x
			));
		
		#endregion Health

		#region Actuals

		public static readonly ReadingDescriptor PackVoltage = new ChartReadingDescriptor(
			new ReadingDescription(
				"Voltage", 
				"The actual battery pack voltage."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, BatteryActualsWrapper.CreateKey(BatteryActualsWrapper.VoltageEntryName) }
			},
			new ReadingValueAccessor(
				b => b.Actuals.Voltage,
				"{0} V"
			),
			new ReadingVisualizer(
				x => (double)x
			));

		public static readonly ReadingDescriptor ActualCurrent = new ChartReadingDescriptor(
			new ReadingDescription(
				"Current", 
				"The current load current."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, BatteryActualsWrapper.CreateKey(BatteryActualsWrapper.ActualCurrentEntryName) }
			},
			new ReadingValueAccessor(
				b => b.Actuals.ActualCurrent,
				"{0} A"
			),
			new ReadingVisualizer(
				x => (double)x
			));

		public static readonly ReadingDescriptor AverageCurrent = new ChartReadingDescriptor(
			new ReadingDescription(
				"Average current", 
				"The average load current."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, BatteryActualsWrapper.CreateKey(BatteryActualsWrapper.AverageCurrentEntryName) }
			},
			new ReadingValueAccessor(
				b => b.Actuals.AverageCurrent,
				"{0} A"
			),
			new ReadingVisualizer(
				x => (double)x
			));

		public static readonly ReadingDescriptor Temperature = new ChartReadingDescriptor(
			new ReadingDescription(
				"Temperature", 
				"The current pack temperature."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, BatteryActualsWrapper.CreateKey(BatteryActualsWrapper.TemperatureEntryName) }
			},
			new ReadingValueAccessor(
				b => b.Actuals.Temperature - 273.15,
				"{0:f1} °C"
			),
			new ReadingVisualizer(
				x => (double)x
			));

		////yield return new ReadingDescriptor<BatteryPack, object>(b => b.Conditions.CellVoltages[0], "Conditions.CellVoltages[0]", "{0} V", "Cell 1 voltage", "The current voltage of the cell 1.");
		////yield return new ReadingDescriptor<BatteryPack, object>(b => b.Conditions.CellVoltages[1], "Conditions.CellVoltages[1]", "{0} V", "Cell 2 voltage", "The current voltage of the cell 2.");
		////yield return new ReadingDescriptor<BatteryPack, object>(b => b.Conditions.CellVoltages[2], "Conditions.CellVoltages[2]", "{0} V", "Cell 3 voltage", "The current voltage of the cell 3.");
		////yield return new ReadingDescriptor<BatteryPack, object>(b => b.Conditions.CellVoltages[3], "Conditions.CellVoltages[3]", "{0} V", "Cell 4 voltage", "The current voltage of the cell 4.");

		#endregion Actuals
	}
}
