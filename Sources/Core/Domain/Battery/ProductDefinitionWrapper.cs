using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.Domain.Battery
{
	public class ProductDefinitionWrapper : DataDictionaryWrapperBase, IProductDefinition
	{
		public const string NamespaceUriName = "ProductDefinitionNS";
		public const string ManufacturerEntryName = "Manufacturer";
		public const string ProductEntryName = "Product";
		public const string ChemistryEntryName = "Chemistry";
		public const string ManufactureDateEntryName = "ManufactureDate";
		public const string SerialNumberEntryName = "SerialNumber";

		public ProductDefinitionWrapper(DataDictionary data)
			: base(data)
		{
		}

		protected override string NamespaceUri
		{
			get { return NamespaceUriName; }
		}


		public string Manufacturer
		{
			get { return this.GetValue<string>(ManufacturerEntryName); }
			set { this.SetValue(ManufacturerEntryName, value); }
		}

		public string Product
		{
			get { return this.GetValue<string>(ProductEntryName); }
			set { this.SetValue(ProductEntryName, value); }
		}

		public string Chemistry
		{
			get { return this.GetValue<string>(ChemistryEntryName); }
			set { this.SetValue(ChemistryEntryName, value); }
		}

		public DateTime ManufactureDate
		{
			get { return this.GetValue<DateTime>(ManufactureDateEntryName); }
			set { this.SetValue(ManufactureDateEntryName, value); }
		}

		public string SerialNumber
		{
			get { return this.GetValue<string>(SerialNumberEntryName); }
			set { this.SetValue(SerialNumberEntryName, value); }
		}

		public static EntryKey CreateKey(string entryName)
		{
			return new EntryKey(NamespaceUriName, entryName);
		}
	}
}
