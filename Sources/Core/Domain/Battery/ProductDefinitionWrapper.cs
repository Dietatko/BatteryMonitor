using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public class ProductDefinitionWrapper : DataDictionaryWrapperBase
	{
		public ProductDefinitionWrapper(ReadingStorage data)
			: base(data)
		{
		}


		public string Manufacturer
		{
			get { return this.GetValue<string>(ManufacturerKey); }
			set { this.SetValue(ManufacturerKey, value); }
		}

		public string Product
		{
			get { return this.GetValue<string>(ProductKey); }
			set { this.SetValue(ProductKey, value); }
		}

		public string Chemistry
		{
			get { return this.GetValue<string>(ChemistryKey); }
			set { this.SetValue(ChemistryKey, value); }
		}

		public DateTime ManufactureDate
		{
			get { return this.GetValue<DateTime>(ManufactureDateKey); }
			set { this.SetValue(ManufactureDateKey, value); }
		}

		public string SerialNumber
		{
			get { return this.GetValue<string>(SerialNumberKey); }
			set { this.SetValue(SerialNumberKey, value); }
		}


		#region Entry keys

		private const string NamespaceUriName = "ProductDefinitionNS";
		private const string ManufacturerEntryName = "Manufacturer";
		private const string ProductEntryName = "Product";
		private const string ChemistryEntryName = "Chemistry";
		private const string ManufactureDateEntryName = "ManufactureDate";
		private const string SerialNumberEntryName = "SerialNumber";

		public static readonly EntryKey ManufacturerKey = CreateKey(ManufacturerEntryName);
		public static readonly EntryKey ProductKey = CreateKey(ProductEntryName);
		public static readonly EntryKey ChemistryKey = CreateKey(ChemistryEntryName);
		public static readonly EntryKey ManufactureDateKey = CreateKey(ManufactureDateEntryName);
		public static readonly EntryKey SerialNumberKey = CreateKey(SerialNumberEntryName);

		private static EntryKey CreateKey(string entryName)
		{
			return new EntryKey(NamespaceUriName, entryName);
		}

		#endregion Entry keys
	}
}
