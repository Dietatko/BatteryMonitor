using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;

using ImpruvIT.BatteryMonitor.WPFApp.ViewLogic;

namespace ImpruvIT.BatteryMonitor.WPFApp.Controls
{
	public class ReadingValueConverter : IValueConverter
	{
		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			if (targetType != typeof(string))
				return System.Convert.ChangeType(value, targetType);

			var descriptor = parameter as ReadingDescriptor;
			if (descriptor == null)
				return value.ToString();

			return String.Format(descriptor.Accessor.FormatString, value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
