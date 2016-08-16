using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

using ImpruvIT.BatteryMonitor.Domain.Battery;
using ImpruvIT.BatteryMonitor.Domain.Descriptors;

namespace ImpruvIT.BatteryMonitor.Protocols
{
	public interface IBatteryAdapter : INotifyPropertyChanged
	{
		Pack Pack { get; }

		Task RecognizeBattery();
		Task UpdateReadings();
		ISubscription SubscribeToUpdates(Action<Pack> notificationConsumer, UpdateFrequency frequency = UpdateFrequency.Normal);

		IEnumerable<ReadingDescriptorGrouping> GetDescriptors();
		event EventHandler DescriptorsChanged;
	}
}