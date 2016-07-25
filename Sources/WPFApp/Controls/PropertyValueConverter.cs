using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using ImpruvIT.BatteryMonitor.WPFApp.ViewLogic;

namespace ImpruvIT.BatteryMonitor.WPFApp.Controls
{
	public class PropertyValueConverter : IMultiValueConverter
	{
		public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var valueDescription = values[0] as IReadingDescription<OldBattery, object>;
			var item = values[1] as OldBattery;

			object value = valueDescription.ValueSelector(item);
			return String.Format(valueDescription.FormatString, value);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
		{
			return new [] { value };
		}
	}
}
