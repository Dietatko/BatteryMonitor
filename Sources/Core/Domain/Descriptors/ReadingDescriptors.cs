using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain.Battery;

namespace ImpruvIT.BatteryMonitor.Domain.Descriptors
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
				{ b => b, ProductDefinitionWrapper.ManufacturerKey }
			},
			new ReadingValueAccessor(
				b => GetValue<string>(b, ProductDefinitionWrapper.ManufacturerKey)
			));

		public static readonly ReadingDescriptor Product = new ReadingDescriptor(
			new ReadingDescription(
				"Product", 
				"The battery pack product name."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, ProductDefinitionWrapper.ProductKey }
			},
			new ReadingValueAccessor(
				b => GetValue<string>(b, ProductDefinitionWrapper.ProductKey)
			));

		public static readonly ReadingDescriptor ManufactureDate = new ReadingDescriptor(
			new ReadingDescription(
				"Manufacture date",
				"The battery pack manufacture date."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, ProductDefinitionWrapper.ManufactureDateKey }
			},
			new ReadingValueAccessor(
				b => GetValue<DateTime>(b, ProductDefinitionWrapper.ManufactureDateKey),
				"{0:d}"
			));

		public static readonly ReadingDescriptor SerialNumber = new ReadingDescriptor(
			new ReadingDescription(
				"Serial number", 
				"The battery pack serial number."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, ProductDefinitionWrapper.SerialNumberKey }
			},
			new ReadingValueAccessor(
				b => GetValue<string>(b, ProductDefinitionWrapper.SerialNumberKey)
			));

		public static readonly ReadingDescriptor Chemistry = new ReadingDescriptor(
			new ReadingDescription(
				"Chemistry", 
				"The battery pack chemistry."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, ProductDefinitionWrapper.ChemistryKey }
			},
			new ReadingValueAccessor(
				b => GetValue<string>(b, ProductDefinitionWrapper.ChemistryKey)
			));

		#endregion Product

		#region Design parameters

		public static readonly ReadingDescriptor NominalVoltage = new ChartReadingDescriptor(
			new ReadingDescription(
				"Nominal voltage", 
				"The nominal voltage of the battery pack."
			),
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, DesignParametersWrapper.NominalVoltageKey }
			},
			new ReadingValueAccessor(
				b => GetValue<float>(b, DesignParametersWrapper.NominalVoltageKey),
				"{0:N1} V"
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
				{ b => b, DesignParametersWrapper.DesignedCapacityKey }
			},
			new ReadingValueAccessor(
				b => GetValue<float>(b, DesignParametersWrapper.DesignedDischargeCurrentKey),
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
				{ b => b, DesignParametersWrapper.MaxDischargeCurrentKey }
			},
			new ReadingValueAccessor(
				b => GetValue<float>(b, DesignParametersWrapper.MaxDischargeCurrentKey),
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
				{ b => b, DesignParametersWrapper.DesignedCapacityKey }
			},
			new ReadingValueAccessor(
				b => GetValue<float>(b, DesignParametersWrapper.DesignedCapacityKey) * 1000,
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
				{ b => b, BatteryHealthWrapper.FullChargeCapacityKey }
			},
			new ReadingValueAccessor(
				b => GetValue<float>(b, BatteryHealthWrapper.FullChargeCapacityKey) * 1000,
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
				{ b => b, BatteryHealthWrapper.CycleCountKey }
			},
			new ReadingValueAccessor(
				b => GetValue<int>(b, BatteryHealthWrapper.CycleCountKey),
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
				{ b => b, BatteryHealthWrapper.CalculationPrecisionKey }
			},
			new ReadingValueAccessor(
				b => GetValue<float>(b, BatteryHealthWrapper.CalculationPrecisionKey) * 100,
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
				{ b => b, BatteryActualsWrapper.VoltageKey }
			},
			new ReadingValueAccessor(
				b => GetValue<float>(b, BatteryActualsWrapper.VoltageKey),
				"{0:N3} V"
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
				{ b => b, BatteryActualsWrapper.ActualCurrentKey }
			},
			new ReadingValueAccessor(
				b => GetValue<float>(b, BatteryActualsWrapper.ActualCurrentKey),
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
				{ b => b, BatteryActualsWrapper.AverageCurrentKey }
			},
			new ReadingValueAccessor(
				b => GetValue<float>(b, BatteryActualsWrapper.AverageCurrentKey),
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
				{ b => b, BatteryActualsWrapper.TemperatureKey }
			},
			new ReadingValueAccessor(
				b => GetValue<float>(b, BatteryActualsWrapper.TemperatureKey) - 273.15,
				"{0:f1} °C"
			),
			new ReadingVisualizer(
				x => (double)x
			));

		#endregion Actuals


		public static T GetValue<T>(BatteryElement element, EntryKey key, T defaultValue = default(T))
		{
			T result = defaultValue;

			IReadingValue readingValue;
			if (element.CustomData.TryGetValue(key, out readingValue))
			{
				if (readingValue.IsDefined)
					result = readingValue.Get<T>();
			}

			return result;
		}
	}
}
