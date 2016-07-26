using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ImpruvIT.BatteryMonitor.WPFApp.Data
{
	public class NullDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate EmptyTemplate { get; set; }
		public DataTemplate ItemTemplate { get; set; }

		/// <summary>
		/// When overridden in a derived class, returns a <see cref="T:System.Windows.DataTemplate"/> based on custom logic.
		/// </summary>
		/// <returns>
		/// Returns a <see cref="T:System.Windows.DataTemplate"/> or null. The default value is null.
		/// </returns>
		/// <param name="item">The data object for which to select the template.</param><param name="container">The data-bound object.</param>
		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			return item != null ? ItemTemplate : EmptyTemplate;
		}
	}
}
