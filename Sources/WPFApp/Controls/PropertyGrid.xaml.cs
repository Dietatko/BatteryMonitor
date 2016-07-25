using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using ImpruvIT.BatteryMonitor.WPFApp.ViewLogic;

namespace ImpruvIT.BatteryMonitor.WPFApp.Controls
{
	/// <summary>
	/// Interaction logic for PropertyGrid.xaml
	/// </summary>
	public partial class PropertyGrid : UserControl
	{
		public PropertyGrid()
		{
			InitializeComponent();
		}

		public static readonly DependencyProperty ItemProperty = DependencyProperty.Register("Item", typeof(object), typeof(PropertyGrid), new UIPropertyMetadata(null, Item_Changed));
		public object Item
		{
			get { return (object)GetValue(ItemProperty); }
			set { SetValue(ItemProperty, value); }
		}
		private static void Item_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			((PropertyGrid)obj).OnItemChanged((object)args.OldValue, (object)args.NewValue);
		}
		protected virtual void OnItemChanged(object oldValue, object newValue)
		{

		}

		public ItemCollection Properties
		{
			get { return this.PropertyList.Items; }
		}

		public static readonly DependencyProperty PropertiesSourceProperty = DependencyProperty.Register("PropertiesSource", typeof(ListBase<IReadingDescription<OldBattery, object>>), typeof(PropertyGrid), new UIPropertyMetadata(null, PropertiesSource_Changed));
		public ListBase<IReadingDescription<OldBattery, object>> PropertiesSource
		{
			get { return (ListBase<IReadingDescription<OldBattery, object>>)GetValue(PropertiesSourceProperty); }
			set { SetValue(PropertiesSourceProperty, value); }
		}
		private static void PropertiesSource_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			((PropertyGrid)obj).OnPropertiesSourceChanged((ListBase<IReadingDescription<OldBattery, object>>)args.OldValue, (ListBase<IReadingDescription<OldBattery, object>>)args.NewValue);
		}
		protected virtual void OnPropertiesSourceChanged(ListBase<IReadingDescription<OldBattery, object>> oldValue, ListBase<IReadingDescription<OldBattery, object>> newValue)
		{
		}
	}
}
