using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using ImpruvIT.Contracts;

namespace ImpruvIT.BatteryMonitor.Hardware
{
	/// <summary>
	/// A bus device discovery service delegating to sub-providers in sequence.
	/// </summary>
	public class SequentialDiscoveryService : IDiscoverDevices
	{
		public SequentialDiscoveryService(IEnumerable<IDiscoverDevices> providers)
		{
			Contract.Requires(providers, "providers").NotToBeNull();

			this.Providers = providers.ToList();
			foreach (var provider in this.Providers)
				provider.ConnectorsChanged += (sender, args) => this.OnConnectorsChanged();
		}

		public IEnumerable<IDiscoverDevices> Providers { get; private set; }

		/// <summary>
		/// Gets all available bus devices.
		/// </summary>
		/// <returns>A list of <see cref="IBusDevice">devices</see> communicating to a bus.</returns>
		public async Task<IEnumerable<IBusDevice>> GetConnectors()
		{
			var devices = new List<IBusDevice>();

			foreach (var provider in Providers)
			{
				var providerDevices = await provider.GetConnectors().ConfigureAwait(false);
				devices.AddRange(providerDevices);
			}

			return devices;
		}

		/// <summary>
		/// Occurs when list of SMBus connectors changes.
		/// </summary>
		public event EventHandler ConnectorsChanged;

		protected virtual void OnConnectorsChanged()
		{
			var handlers = this.ConnectorsChanged;
			if (handlers != null)
				handlers(this, EventArgs.Empty);
		}
	}
}
