using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImpruvIT.BatteryMonitor.Hardware.Ftdi
{
	/// <summary>
	/// A FTDI-based bus device using the MPSSE mode.
	/// </summary>
	public class MpsseDevice : IBusDevice
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="MpsseDevice"/> class.
		/// </summary>
		/// <param name="deviceNode">A FTDI device info node.</param>
		/// <param name="deviceChannelIndex">A channel index.</param>
		public MpsseDevice(NativeMethods.FT_DEVICE_LIST_INFO_NODE deviceNode, int deviceChannelIndex)
		{
			this.DeviceNode = deviceNode;
			this.DeviceChannelIndex = deviceChannelIndex;
			this.m_properties = BuildProperties();
		}


		/// <summary>
		/// A FTDI device node.
		/// </summary>
		protected NativeMethods.FT_DEVICE_LIST_INFO_NODE DeviceNode { get; private set; }

		/// <summary>
		/// A FTDI device node.
		/// </summary>
		protected int DeviceChannelIndex { get; private set; }

		/// <inheritdoc />
		public string Name
		{
			get { return string.Format("{0} ({1}) - {2}", this.DeviceNode.Description, this.DeviceNode.SerialNumber, this.DeviceChannelIndex); }
		}

		/// <inheritdoc />
		public string Type
		{
			get { return "FTDI MPSEE"; }
		}

		/// <inheritdoc />
		public IDictionary<string, string> Properties
		{
			get { return new Dictionary<string, string>(this.m_properties); }
		}
		private readonly Dictionary<string, string> m_properties;


		/// <summary>
		/// Connects to the bus.
		/// </summary>
		/// <returns>A completion task providing connection to the device.</returns>
		public Task<IBusConnection> Connect()
		{
			var connection = new MpsseConnection();
			var task = connection.Connect(this.DeviceNode.SerialNumber, this.DeviceChannelIndex);

			return task.ContinueWith<IBusConnection>(x => connection);
		}

		private Dictionary<string, string> BuildProperties()
		{
			var result = new Dictionary<string, string>();

			var allFlags = new[] { NativeMethods.FT_FLAGS.FT_FLAGS_OPENED, NativeMethods.FT_FLAGS.FT_FLAGS_HISPEED };
			var flagText = String.Join(" | ", allFlags.Where(f => (this.DeviceNode.Flags & f) == f).Select(f => f.ToString().Substring(9)));

			result.Add(Constants.FlagsProperty, flagText);
			result.Add(Constants.TypeProperty, this.DeviceNode.Type.ToString());
			result.Add(Constants.IdProperty, String.Format("{0:x}", this.DeviceNode.ID));
			result.Add(Constants.LocationIdProperty, String.Format("{0:x}", this.DeviceNode.LocId));
			result.Add(Constants.SerialNumberProperty, this.DeviceNode.SerialNumber);
			result.Add(Constants.DescriptionProperty, this.DeviceNode.Description);

			return result;
		}
	}
}
