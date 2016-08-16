using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Domain.Battery;
using ImpruvIT.BatteryMonitor.Domain.Descriptors;
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
				{ b => b, LtDataWrapper.ChipCountKey }
			},
			new ReadingValueAccessor(
				b => ReadingDescriptors.GetValue<int>(b, LtDataWrapper.ChipCountKey)
			));

		public static readonly ReadingDescriptor CellCount = new ReadingDescriptor(
			new ReadingDescription(
				"Cell count", 
				"A number of cells in the battery pack."
			), 
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, LtDataWrapper.CellCountKey }
			},
			new ReadingValueAccessor(
				b => ReadingDescriptors.GetValue<int>(b, LtDataWrapper.CellCountKey)
			));

		public static readonly ReadingDescriptor SumOfCellVoltages = new ReadingDescriptor(
			new ReadingDescription(
				"Sum of cell voltages",
				"A sum of all cell voltages. Difference to pack voltage shows losses."
			),
			new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
				{ b => b, LtDataWrapper.SumOfCellVoltagesKey }
			},
			new ReadingValueAccessor(
				b => ReadingDescriptors.GetValue<float>(b, LtDataWrapper.SumOfCellVoltagesKey),
				"{0:N3} V"
			));

		public static ReadingDescriptor CreateSingleChipCellVoltageDescriptor(int cellIndex)
		{
			return new ReadingDescriptor(
				new ReadingDescription(
					String.Format("Cell {0} voltage", cellIndex + 1),
					String.Format("A voltage of the cell {0}.", cellIndex + 1)
				),
				new Dictionary<Func<BatteryElement, BatteryElement>, EntryKey> {
					{ b => ((Pack)b)[cellIndex], BatteryActualsWrapper.VoltageKey }
				},
				new ReadingValueAccessor(
					b => ReadingDescriptors.GetValue<float>(((Pack)b)[cellIndex], BatteryActualsWrapper.VoltageKey),
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
					{ b => ((Pack)b).SubElements
							.OfType<ChipPack>()
							.Single(x => x.ChainIndex == chipIndex)[cellIndex], 
						BatteryActualsWrapper.VoltageKey }
				},
				new ReadingValueAccessor(

					b => ReadingDescriptors.GetValue<float>(
						((Pack)b).SubElements
							.OfType<ChipPack>()
							.Single(x => x.ChainIndex == chipIndex)[cellIndex], 
						BatteryActualsWrapper.VoltageKey),
					"{0:N3} V"
				));
		}
	}
}
