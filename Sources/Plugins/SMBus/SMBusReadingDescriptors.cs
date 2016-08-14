using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Domain.Description;

namespace ImpruvIT.BatteryMonitor.Protocols.SMBus
{
	public static class SMBusReadingDescriptors
	{
		public static readonly ReadingDescriptor SpecificationVersion = new ReadingDescriptor(
			new ReadingDescription(
				"SpecificationVersion", 
				"The SMBus specification version the battery pack conforms to."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, SMBusDataWrapper.CreateKey(SMBusDataWrapper.SpecificationVersionEntryName) }
			},
			new ReadingValueAccessor(
				b => new SMBusDataWrapper(b.CustomData).SpecificationVersion
			));

		public static readonly ReadingDescriptor CellCount = new ReadingDescriptor(
			new ReadingDescription(
				"Cell count", 
				"A number of cells in the battery pack."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, SMBusDataWrapper.CreateKey(SMBusDataWrapper.CellCountEntryName) }
			},
			new ReadingValueAccessor(
				b => new SMBusDataWrapper(b.CustomData).CellCount
			));

		public static ReadingDescriptor CreateCellVoltageDescriptor(int cellIndex)
		{
			return new ReadingDescriptor(
				new ReadingDescription(
					String.Format("Cell {0} voltage", cellIndex + 1),
					String.Format("A voltage of the cell {0}.", cellIndex + 1)
				),
				new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
					{ b => ((BatteryPack)b)[cellIndex], BatteryActualsWrapper.CreateKey(BatteryActualsWrapper.VoltageEntryName) }
				},
				new ReadingValueAccessor(
					b => ((BatteryPack)b)[cellIndex].Actuals.Voltage,
					"{0:N3} V"
				));
		}
	}
}
