using System;
using System.Collections.Generic;
using System.Linq;

using ImpruvIT.Contracts;

using ImpruvIT.BatteryMonitor.Domain;

namespace ImpruvIT.BatteryMonitor.WPFApp.ViewLogic
{
	public class ReadingDescriptor
	{
		public ReadingDescriptor(
			ReadingDescription description,
			IDictionary<Func<BatteryElement, BatteryElement>, EntryKey> sourceKeys, 
			ReadingValueAccessor accessor)
		{
			this.Description = description;
			this.SourceKeys = sourceKeys;
			this.Accessor = accessor;
		}

		public ReadingDescription Description { get; private set; }
		public IDictionary<Func<BatteryElement, BatteryElement>, EntryKey> SourceKeys { get; private set; }
		public ReadingValueAccessor Accessor { get; private set; }
	}

	public class GraphReadingDescriptor : ReadingDescriptor
	{
		public GraphReadingDescriptor(ReadingDescription description, IDictionary<Func<BatteryElement, BatteryElement>, EntryKey> sourceKeys, ReadingValueAccessor accessor, ReadingVisualizer graphVisualiser)
			: base(description, sourceKeys, accessor)
		{
			this.GraphVisualiser = graphVisualiser;
		}

		public ReadingVisualizer GraphVisualiser { get; private set; }
	}

	public class ReadingDescription
	{
		public ReadingDescription(string title, string description)
		{
			Contract.Requires(title, "title").IsNotNull().IsNotEmpty();
			Contract.Requires(description, "description").IsNotNull();

			this.Title = title;
			this.Description = description;
		}

		public string Title { get; private set; }
		public string Description { get; private set; }
	}

	public class ReadingValueAccessor
	{
		private const string DefaultFormatString = "{0}";

		public ReadingValueAccessor(Func<BatteryElement, object> valueSelector, string formatString = DefaultFormatString)
		{
			Contract.Requires(valueSelector, "valueSelector").IsNotNull();
			Contract.Requires(formatString, "formatString").IsNotNull();

			this.ValueSelector = valueSelector;
			this.FormatString = formatString;
		}

		public Func<BatteryElement, object> ValueSelector { get; private set; }
		public string FormatString { get; private set; }
	}

	public class ReadingVisualizer
	{
		public ReadingVisualizer(Func<object, double> graphValueConverter)
		{
			Contract.Requires(graphValueConverter, "graphValueConverter").IsNotNull();

			this.GraphValueConverter = graphValueConverter;
		}

		public Func<object, double> GraphValueConverter { get; private set; }
	}


}
