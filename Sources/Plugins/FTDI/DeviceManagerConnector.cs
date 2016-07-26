using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FTD2XX_NET;

namespace ImpruvIT.BatteryMonitor.Hardware.Ftdi
{
	/// <summary>
	/// A FTDI-based bus connector.
	/// </summary>
	public class DeviceManagerConnector : IBusDevice
	{
		public const string FlagsProperty = "Flags";
		public const string TypeProperty = "Type";
		public const string IdProperty = "Id";
		public const string LocationIdProperty = "LocationId";
		public const string SerialNumberProperty = "SerialNumber";
		public const string DescriptionProperty = "Description";

		/// <summary>
		/// Initializes a new instance of the <see cref="DeviceManagerConnector"/> class.
		/// </summary>
		/// <param name="deviceNode">A FTDI device info node.</param>
		public DeviceManagerConnector(FTDI.FT_DEVICE_INFO_NODE deviceNode)
		{
			this.DeviceNode = deviceNode;
			this.m_properties = BuildProperties(deviceNode);
		}


		/// <summary>
		/// A FTDI device node.
		/// </summary>
		protected FTDI.FT_DEVICE_INFO_NODE DeviceNode { get; private set; }

		/// <summary>
		/// Gets a name of the connector.
		/// </summary>
		public string Name
		{
			get { return this.m_properties[SerialNumberProperty]; }
		}

		/// <summary>
		/// Gets a type of the connector.
		/// </summary>
		public string Type
		{
			get { return "FTDI_D2XX"; }
		}

		/// <summary>
		/// Gets a list of custom properties of the connector.
		/// </summary>
		public IDictionary<string, string> Properties
		{
			get { return new Dictionary<string, string>(this.m_properties); }
		}
		private readonly Dictionary<string, string> m_properties;


		/// <summary>
		/// Connects to the connector.
		/// </summary>
		/// <returns>A completion task providing connection to the device.</returns>
		public Task<IBusConnection> Connect()
		{
			var result = new DeviceManagerConnection();
			result.Connect(this.DeviceNode);
			return Task.FromResult<IBusConnection>(result);
		}

		private Dictionary<string, string> BuildProperties(FTDI.FT_DEVICE_INFO_NODE deviceNode)
		{
			var result = new Dictionary<string, string>();

			var allFlags = new[] { FTDI.FT_FLAGS.FT_FLAGS_OPENED, FTDI.FT_FLAGS.FT_FLAGS_OPENED };
			var flagText = String.Join(" | ", allFlags.Where(f => (deviceNode.Flags & f) == f).Select(f => f.ToString().Substring(9)));

			result.Add(FlagsProperty, flagText);
			result.Add(TypeProperty, deviceNode.Type.ToString());
			result.Add(IdProperty, String.Format("{0:x}", deviceNode.ID));
			result.Add(LocationIdProperty, String.Format("{0:x}", deviceNode.LocId));
			result.Add(SerialNumberProperty, deviceNode.SerialNumber);
			result.Add(DescriptionProperty, deviceNode.Description);

			return result;
		}
	}
}
