using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.WPFApp.ViewLogic;

namespace ImpruvIT.BatteryMonitor.WPFApp.Controls
{
	/// <summary>
	/// Interaction logic for PropertyGrid.xaml
	/// </summary>
	public partial class PropertyGrid
	{
		public PropertyGrid()
		{
			this.ViewLogic = new PropertyGridViewLogic();

			InitializeComponent();
		}

		public PropertyGridViewLogic ViewLogic { get; private set; }

		public static readonly DependencyProperty ItemProperty = DependencyProperty.Register("Item", typeof(BatteryElement), typeof(PropertyGrid), new UIPropertyMetadata(null, Item_Changed));
		public BatteryElement Item
		{
			get { return (BatteryElement)GetValue(ItemProperty); }
			set { SetValue(ItemProperty, value); }
		}
		private static void Item_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			((PropertyGrid)obj).OnItemChanged((BatteryElement)args.OldValue, (BatteryElement)args.NewValue);
		}
		protected virtual void OnItemChanged(BatteryElement oldValue, BatteryElement newValue)
		{
			this.ViewLogic.Battery = newValue;
		}

		public static readonly DependencyProperty PropertiesSourceProperty = DependencyProperty.Register("PropertiesSource", typeof(IEnumerable<ReadingDescriptor>), typeof(PropertyGrid), new UIPropertyMetadata(null, PropertiesSource_Changed));
		public IEnumerable<ReadingDescriptor> PropertiesSource
		{
			get { return (IEnumerable<ReadingDescriptor>)GetValue(PropertiesSourceProperty); }
			set { SetValue(PropertiesSourceProperty, value); }
		}
		private static void PropertiesSource_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			((PropertyGrid)obj).OnPropertiesSourceChanged((IEnumerable<ReadingDescriptor>)args.OldValue, (IEnumerable<ReadingDescriptor>)args.NewValue);
		}
		protected virtual void OnPropertiesSourceChanged(IEnumerable<ReadingDescriptor> oldValue, IEnumerable<ReadingDescriptor> newValue)
		{
			this.ViewLogic.Descriptors = newValue;
		}

		public ItemCollection Properties
		{
			get { return this.PropertyList.Items; }
		}
	}
}
