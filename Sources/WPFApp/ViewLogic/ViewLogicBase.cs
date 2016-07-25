using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImpruvIT.BatteryMonitor.WPFApp.ViewLogic
{
	public abstract class ViewLogicBase : INotifyPropertyChanged
	{
		/// <inheritdoc />
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Fires the <see cref="PropertyChanged"/> event.
		/// </summary>
		/// <param name="args">The <see cref="PropertyChangedEventArgs"/> that contains the event data.</param>
		protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
		{
			PropertyChangedEventHandler handlers = this.PropertyChanged;
			if (handlers != null)
				handlers(this, args);
		}
	}
}
