using System;
using System.Collections.Generic;
using System.Linq;

namespace ImpruvIT.BatteryMonitor.WPFApp.ViewLogic
{
	public interface IReadingDescription<in TObject, out TValue>
	{
		Func<TObject, TValue> ValueSelector { get; }
		string BindingPath { get; }
		string FormatString { get; }
		string Title { get; }
		string Description { get; }
	}

	public class ReadingDescription<TObject, TValue> : IReadingDescription<TObject, TValue>
	{
		private const string DefaultFormatString = "{0}";
		private const string DefaultDescription = "";

		public ReadingDescription(Func<TObject, TValue> valueSelector, string bindingPath, string title, string description = DefaultDescription)
			: this(valueSelector, bindingPath, DefaultFormatString, title, description)
		{
		}

		public ReadingDescription(Func<TObject, TValue> valueSelector, string bindingPath, string formatString, string title, string description = DefaultDescription)
		{
			this.ValueSelector = valueSelector;
			this.BindingPath = bindingPath;
			this.FormatString = formatString;
			this.Title = title;
			this.Description = description;
		}

		public Func<TObject, TValue> ValueSelector { get; private set; }
		public string BindingPath { get; private set; }
		public string FormatString { get; private set; }
		public string Title { get; private set; }
		public string Description { get; private set; }
	}
}
