using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Domain.Description;
using ImpruvIT.BatteryMonitor.WPFApp.ViewLogic;

namespace ImpruvIT.BatteryMonitor.WPFApp.Controls
{
	/// <summary>
	/// Interaction logic for ReadingsGrid.xaml
	/// </summary>
	public partial class ReadingsGrid
	{
		public ReadingsGrid()
		{
			this.ViewLogic = new ReadingsGridViewLogic();

			InitializeComponent();
		}

		public ReadingsGridViewLogic ViewLogic { get; private set; }

		public static readonly DependencyProperty ItemProperty = DependencyProperty.Register("Item", typeof(BatteryElement), typeof(ReadingsGrid), new UIPropertyMetadata(null, Item_Changed));
		public BatteryElement Item
		{
			get { return (BatteryElement)GetValue(ItemProperty); }
			set { SetValue(ItemProperty, value); }
		}
		private static void Item_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			((ReadingsGrid)obj).OnItemChanged((BatteryElement)args.OldValue, (BatteryElement)args.NewValue);
		}
		protected virtual void OnItemChanged(BatteryElement oldValue, BatteryElement newValue)
		{
			this.ViewLogic.Battery = newValue;
		}

		public static readonly DependencyProperty PropertiesSourceProperty = DependencyProperty.Register("PropertiesSource", typeof(IEnumerable<ReadingDescriptorGrouping>), typeof(ReadingsGrid), new UIPropertyMetadata(null, PropertiesSource_Changed));
		public IEnumerable<ReadingDescriptorGrouping> PropertiesSource
		{
			get { return (IEnumerable<ReadingDescriptorGrouping>)GetValue(PropertiesSourceProperty); }
			set { SetValue(PropertiesSourceProperty, value); }
		}
		private static void PropertiesSource_Changed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			((ReadingsGrid)obj).OnPropertiesSourceChanged((IEnumerable<ReadingDescriptorGrouping>)args.OldValue, (IEnumerable<ReadingDescriptorGrouping>)args.NewValue);
		}
		protected virtual void OnPropertiesSourceChanged(IEnumerable<ReadingDescriptorGrouping> oldValue, IEnumerable<ReadingDescriptorGrouping> newValue)
		{
			this.ViewLogic.Descriptors = newValue;
		}

		public ItemCollection Properties
		{
			get { return this.GroupTabControl.Items; }
		}
	}
}
