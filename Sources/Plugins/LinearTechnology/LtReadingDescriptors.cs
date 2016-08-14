using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Domain.Description;
using ImpruvIT.BatteryMonitor.Protocols.LinearTechnology.LTC6804;

namespace ImpruvIT.BatteryMonitor.Protocols.LinearTechnology
{
	public static class LtReadingDescriptors
	{
		public static readonly ReadingDescriptor ChipCount = new ReadingDescriptor(
			new ReadingDescription(
				"Pack count",
				"A number of packs in the overall battery pack."
			),
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, LtDataWrapper.CreateKey(LtDataWrapper.CellCountEntryName) }
			},
			new ReadingValueAccessor(
				b => new LtDataWrapper(b.CustomData).CellCount
			));

		public static readonly ReadingDescriptor CellCount = new ReadingDescriptor(
			new ReadingDescription(
				"Cell count", 
				"A number of cells in the battery pack."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, LtDataWrapper.CreateKey(LtDataWrapper.CellCountEntryName) }
			},
			new ReadingValueAccessor(
				b => new LtDataWrapper(b.CustomData).CellCount
			));

		public static ReadingDescriptor CreateSingleChipCellVoltageDescriptor(int cellIndex)
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

		public static ReadingDescriptor CreateChainCellVoltageDescriptor(int chipIndex, int cellIndex)
		{
			return new ReadingDescriptor(
				new ReadingDescription(
					String.Format("Cell {0}.{1} voltage", chipIndex + 1, cellIndex + 1),
					String.Format("A voltage of the cell {0} on chip {1}.", cellIndex + 1, chipIndex + 1)
				),
				new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
					{ b => ((BatteryPack)b).SubElements
							.OfType<ChipPack>()
							.Single(x => x.ChainIndex == chipIndex)[cellIndex], 
						BatteryActualsWrapper.CreateKey(BatteryActualsWrapper.VoltageEntryName) }
				},
				new ReadingValueAccessor(
					b => ((BatteryPack)b).SubElements
							.OfType<ChipPack>()
							.Single(x => x.ChainIndex == chipIndex)[cellIndex]
							.Actuals.Voltage,
					"{0:N3} V"
				));
		}
	}
}
