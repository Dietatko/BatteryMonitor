using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Domain.Description;
using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.WPFApp.ViewLogic
{
	public class BatteryReadingProvider : INotifyPropertyChanged
	{
		private readonly Dictionary<BatteryElement, List<EntryKey>> m_sourceKeys;

		public BatteryReadingProvider(BatteryElement batteryElement, ReadingDescriptor descriptor)
		{
			Contract.Requires(batteryElement, "batteryElement").NotToBeNull();
			Contract.Requires(descriptor, "descriptor").NotToBeNull();

			this.BatteryElement = batteryElement;
			this.Descriptor = descriptor;

			this.m_sourceKeys = this.Descriptor.SourceKeys
				.GroupBy(x => x.Key(this.BatteryElement))
				.ToDictionary(x => x.Key, x => x.Select(y => y.Value).ToList());
			var sourceElements = m_sourceKeys.Keys
				.Distinct()
				.ToArray();

			foreach (var element in sourceElements)
				element.ValueChanged += this.OnValueChanged;
		}

		public BatteryElement BatteryElement { get; set; }
		public ReadingDescriptor Descriptor { get; set; }

		public object Value
		{
			get { return this.Descriptor.Accessor.ValueSelector(this.BatteryElement); }
		}

		public string FormattedValue
		{
			get { return String.Format(this.Descriptor.Accessor.FormatString, this.Value); }
		}

		private void OnValueChanged(object sender, EntryKey changedKey)
		{
			var element = sender as BatteryElement;
			if (element == null)
				return;

			List<EntryKey> keys;
			if (!this.m_sourceKeys.TryGetValue(element, out keys))
				return;

			if (keys.Contains(changedKey))
			{
				this.OnValueChanged("Value");
				this.OnValueChanged("FormattedValue");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnValueChanged(string propertyName)
		{
			var handlers = this.PropertyChanged;
			if (handlers != null)
				handlers(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
