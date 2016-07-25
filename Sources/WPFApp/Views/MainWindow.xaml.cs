using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using ImpruvIT.BatteryMonitor.WPFApp.ViewLogic;
using Telerik.Charting;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ChartView;
using Telerik.Windows.Controls.Data.PropertyGrid;

namespace ImpruvIT.BatteryMonitor.WPFApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
		{
			this.Logic = new MainViewLogic();
			this.DataContext = this.Logic;
	        
            InitializeComponent();
        }

		public static readonly DependencyProperty LogicProperty = DependencyProperty.Register("Logic", typeof(MainViewLogic), typeof(MainWindow), new UIPropertyMetadata(null));
		public MainViewLogic Logic
		{
			get { return (MainViewLogic)GetValue(LogicProperty); }
			set { SetValue(LogicProperty, value); }
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			this.Logic.DiscoverBusDevices();
		}

	    private void RefreshConnections_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	    {
		    e.CanExecute = true;
	    }

	    private void RefreshConnections_Execute(object sender, ExecutedRoutedEventArgs e)
	    {
			this.Logic.DiscoverBusDevices();
	    }

	    private void Chart_OnInitialized(object sender, EventArgs e)
	    {
		    RadCartesianChart chart = (RadCartesianChart)sender;

		    CartesianAxis voltageAxis = chart.VerticalAxis;
			chart.Series.Add(this.CreateSerie(c => c.CellVoltages[0], this.Resources["VoltageBrush"] as Brush));
			chart.Series.Add(this.CreateSerie(c => c.CellVoltages[0] + c.CellVoltages[1], this.Resources["VoltageBrush"] as Brush));
			chart.Series.Add(this.CreateSerie(c => c.CellVoltages[0] + c.CellVoltages[1] + c.CellVoltages[2], this.Resources["VoltageBrush"] as Brush));

			CartesianAxis currentAxis = this.CreateVerticalAxis("Current", this.Resources["CurrentBrush"] as Brush);
			//currentAxis.MajorStep = 1;
			currentAxis.HorizontalLocation = AxisHorizontalLocation.Right;
			chart.Series.Add(this.CreateSerie(c => c.Current, this.Resources["CurrentBrush"] as Brush, currentAxis));

			CartesianAxis temperatureAxis = this.CreateVerticalAxis("Temperature", this.Resources["CapacityBrush"] as Brush);
		    temperatureAxis.HorizontalLocation = AxisHorizontalLocation.Right;
			chart.Series.Add(this.CreateSerie(c => c.Temperature - 273.15, this.Resources["CapacityBrush"] as Brush, temperatureAxis));

	    }

		private CartesianSeries CreateSerie<T>(Func<BatteryConditions, T> valueSelector, Brush brush, CartesianAxis verticalAxis = null)
		{
			Func<DateTime, int> timeToSecsFunc = dt => (int)(dt - BatteryLogic.BaseTime).TotalSeconds;

			ScatterLineSeries result = new ScatterLineSeries();

			result.SetBinding(ChartSeries.ItemsSourceProperty, new Binding("Readings"));
			result.XValueBinding = new GenericDataPointBinding<ConditionsRecord, int>() { ValueSelector = r => timeToSecsFunc(r.Timestamp) };
			result.YValueBinding = new GenericDataPointBinding<ConditionsRecord, T>() { ValueSelector = r => valueSelector(r.Conditions) };
			
			result.VerticalAxis = verticalAxis;

			result.Stroke = brush;

			return result;
		}

		private CartesianAxis CreateVerticalAxis(string name, Brush brush)
		{
			var result = new LinearAxis();
			result.Title = name;
			result.ElementBrush = brush;

			return result;
		}

	    private void Chart_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			RadCartesianChart chart = (RadCartesianChart)sender;
		    var logic = ((BatteryLogic)chart.DataContext);

			Func<DateTime, int> timeToSecsFunc = dt => (int)(dt - BatteryLogic.BaseTime).TotalSeconds;
			logic.Readings.CollectionChanged += (s, a) => Dispatcher.Invoke(() => 
			{
				var axis = (LinearAxis)chart.HorizontalAxis;
				var allTimestamps = logic.Readings.Select(x => x.Timestamp).ToList();

				if (allTimestamps.Count > 0)
				{
					axis.Minimum = timeToSecsFunc(allTimestamps.Min());
					axis.Maximum = timeToSecsFunc(allTimestamps.Max()) + 10;
				}
				else
				{
					axis.Minimum = Double.NegativeInfinity;
					axis.Maximum = Double.PositiveInfinity;
				}
			});
	    }
    }
}
