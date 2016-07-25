using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

namespace ImpruvIT.BatteryMonitor.WPFApp.ViewLogic
{
	public class CustomFormatConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (targetType != typeof(string))
				return null;

			string formatString = parameter as string;
			formatString = formatString ?? "{0}";

			return String.Format(formatString, value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return value;
		}
	}
}
