using System;
using System.ComponentModel;
using System.Threading.Tasks;
using ImpruvIT.BatteryMonitor.Domain;

namespace ImpruvIT.BatteryMonitor.Protocols
{
	public interface IBatteryPackAdapter : INotifyPropertyChanged
	{
		BatteryPack Pack { get; }

		Task RecognizeBattery();
		Task ReadHealth();
		Task ReadActuals();
		ISubscription SubscribeToUpdates(Action<BatteryPack> notificationConsumer, UpdateFrequency frequency = UpdateFrequency.Normal);
	}
}