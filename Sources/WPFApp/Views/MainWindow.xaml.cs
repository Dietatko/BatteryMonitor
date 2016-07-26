using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using LiveCharts.Configurations;
using LiveCharts.Helpers;
using LiveCharts.Wpf;

using ImpruvIT.BatteryMonitor.WPFApp.ViewLogic;

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
			var chart = (CartesianChart)sender;
			chart.AxisX[0].LabelFormatter = x => new DateTime((long)x).ToShortTimeString();
		}

		private void Chart_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			var chart = (CartesianChart)sender;

			var logic = e.OldValue as BatteryLogic;
			if (logic != null)
			{
				logic.ActualsHistory.CollectionChanged -= (oldValue, newValue) => this.OnActualsChanged(chart, oldValue, newValue);
			}

			var xAxis = chart.AxisX[0];
			xAxis.LabelFormatter = x => new DateTime((long)x).ToLongTimeString();
			xAxis.MinValue = DateTime.UtcNow.Ticks;
			xAxis.MaxValue = DateTime.UtcNow.AddMinutes(2).Ticks;

			VoltageSeries.Configuration = Mappers.Xy<ActualsSnapshot>()
				.X(x => (double)x.Timestamp.Ticks)
				.Y(x => x.Actuals.PackVoltage);

			CurrentSeries.Configuration = Mappers.Xy<ActualsSnapshot>()
				.X(x => (double)x.Timestamp.Ticks)
				.Y(x => x.Actuals.ActualCurrent);

			TemperatureSeries.Configuration = Mappers.Xy<ActualsSnapshot>()
				.X(x => (double)x.Timestamp.Ticks)
				.Y(x => x.Actuals.Temperature - 273.15);

			logic = e.NewValue as BatteryLogic;
			if (logic != null)
			{
				logic.ActualsHistory.CollectionChanged += (oldValue, newValue) => this.OnActualsChanged(chart, oldValue, newValue);
			}
		}

	    private void OnActualsChanged(CartesianChart chart, IEnumerable<ActualsSnapshot> oldValue, IEnumerable<ActualsSnapshot> newValue)
	    {
		    Dispatcher.Invoke(() =>
				{
					var logic = chart.DataContext as BatteryLogic;
					if (logic == null)
						return;

					var axis = chart.AxisX[0];
					var allTimestamps = logic.ActualsHistory.Select(x => x.Timestamp).ToList();

					DateTime minTime, maxTime;
					if (allTimestamps.Count > 0)
					{
						minTime = allTimestamps.Min();
						maxTime = allTimestamps.Max();
					}
					else
						minTime = maxTime = DateTime.UtcNow;

					if (maxTime - minTime < TimeSpan.FromMinutes(2))
						maxTime = minTime + TimeSpan.FromMinutes(2);

					axis.MinValue = minTime.Ticks;
					axis.MaxValue = maxTime.Ticks;
				});
	    }

		//private void Chart_OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
		//{
		//	RadCartesianChart chart = (RadCartesianChart)sender;
		//	var logic = ((BatteryLogic)chart.DataContext);

		//	Func<DateTime, int> timeToSecsFunc = dt => (int)(dt - BatteryLogic.BaseTime).TotalSeconds;
		//	logic.Readings.CollectionChanged += (s, a) => Dispatcher.Invoke(() => 
		//	{
		//		var axis = (LinearAxis)chart.HorizontalAxis;
		//		var allTimestamps = logic.Readings.Select(x => x.Timestamp).ToList();

		//		if (allTimestamps.Count > 0)
		//		{
		//			axis.Minimum = timeToSecsFunc(allTimestamps.Min());
		//			axis.Maximum = timeToSecsFunc(allTimestamps.Max()) + 10;
		//		}
		//		else
		//		{
		//			axis.Minimum = Double.NegativeInfinity;
		//			axis.Maximum = Double.PositiveInfinity;
		//		}
		//	});
		//}
	    
    }
}
