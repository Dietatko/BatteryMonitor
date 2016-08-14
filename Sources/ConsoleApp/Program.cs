using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FTD2XX_NET;
using ImpruvIT;

using ImpruvIT.BatteryMonitor.Domain;
using ImpruvIT.BatteryMonitor.Hardware;
using ImpruvIT.BatteryMonitor.Hardware.Ftdi;
using ImpruvIT.BatteryMonitor.Hardware.Ftdi.I2C;
using ImpruvIT.BatteryMonitor.Protocols;
using ImpruvIT.BatteryMonitor.Protocols.LinearTechnology;
using ImpruvIT.BatteryMonitor.Protocols.SMBus;
using NativeMethods = ImpruvIT.BatteryMonitor.Hardware.Ftdi.NativeMethods;
using SPI = ImpruvIT.BatteryMonitor.Hardware.Ftdi.SPI;
using LTC6804 = ImpruvIT.BatteryMonitor.Protocols.LinearTechnology.LTC6804;

namespace ImpruvIt.BatteryMonitor.ConsoleApp
{
	class Program
	{
		public static string DisabledText = "Disabled";

		static void Main()
		{
			log4net.Config.XmlConfigurator.Configure();

			//TestI2C();
			//TestSPI();
			TestLtc6804();

			// Start monitor thread;
			//MonitorBattery();
		}

		private static void MonitorBattery()
		{
			// Discover bus devices
			IEnumerable<IDiscoverDevices> dicoveryServices = GetDiscoveryServices();
			var discoveryTask = Task.WhenAll(dicoveryServices.Select(x => x.GetConnectors()));
			var allConnectors = discoveryTask.Result.SelectMany(x => x);

			var connector = allConnectors.FirstOrDefault();
			if (connector == null)
			{
				Console.WriteLine("No SMBus connector found.");
				return;
			}

			Console.WriteLine("Connecting to SMBus '{0}' of type '{1}'.", connector.Name, connector.Type);

			var connection = connector.Connect().Result;
			try
			{
				//uint address = DiscoverDevices(connection);
				uint address = 0x2A;

				var batteryAdapter = new BatteryAdapter(new SMBusInterface((ICommunicateToAddressableBus)connection), address);
				batteryAdapter.RecognizeBattery().Wait();
				var batteryPack = batteryAdapter.Pack;

				Console.WriteLine("Battery found at address {0}:", address);
				var product = batteryPack.Product;
				var parameters = batteryPack.DesignParameters;
				Console.WriteLine("Manufacturer:             {0}", product.Manufacturer);
				Console.WriteLine("Product:                  {0}", product.Product);
				Console.WriteLine("Chemistry:                {0}", product.Chemistry);
				Console.WriteLine("Manufacture date:         {0}", product.ManufactureDate.ToShortDateString());
				Console.WriteLine("Serial number:            {0}", product.SerialNumber);
				//Console.WriteLine("Specification version:    {0}", battery.Information.SpecificationVersion.ToString(2));
				Console.WriteLine("Cell count:               {0} cells", batteryPack.ElementCount);
				Console.WriteLine("Nominal voltage:          {0} V", parameters.NominalVoltage);
				Console.WriteLine("DesignedDischargeCurrent: {0} A", parameters.DesignedDischargeCurrent);
				Console.WriteLine("MaxDischargeCurrent:      {0} A", parameters.MaxDischargeCurrent);
				Console.WriteLine("Designed capacity:        {0:N0} mAh", parameters.DesignedCapacity * 1000);
				//Console.WriteLine("Voltage scale:            {0}x", battery.Information.VoltageScale);
				//Console.WriteLine("Current scale:            {0}x", battery.Information.CurrentScale);
				Console.WriteLine();

				batteryAdapter.UpdateReadings().Wait();
				var health = batteryPack.Health;
				Console.WriteLine("Current battery status:");
				Console.WriteLine("Full charge capacity:     {0:N0} mAh", health.FullChargeCapacity * 1000);
				Console.WriteLine("Cycle count:              {0}", health.CycleCount);
				Console.WriteLine("Calculation precision:    {0}", health.CalculationPrecision);
				//ReportAlarmSettings(battery);
				//Console.WriteLine("Battery mode:             {0}", battery.Status.BatteryMode);
				//Console.WriteLine("Battery status:           {0}", battery.Status.BatteryStatus);
				Console.WriteLine();

				//batteryAdapter.ReadActuals().Wait();
				//PrintActuals(battery);

				//batteryAdapter.ReadActuals(
				//		bc => bc.Voltage,
				//		bc => bc.Current
				//	);

				// TestAllCommands(connection, address);

				//connection.WriteWordCommand(address, SMBusCommandIds.RemainingCapacityAlarm, 78);
				//var val = connection.ReadWordCommand(address, SMBusCommandIds.RemainingCapacityAlarm);

				//ushort voltage = connection.ReadWordCommand(address, 0x09);

				var subscription = batteryAdapter.SubscribeToUpdates(PrintActuals, UpdateFrequency.Normal);

				Console.ReadLine();

				subscription.Unsubscribe();
			}
			finally
			{
				connection.Disconnect().Wait();
			}
		}

		private static void PrintActuals(BatteryPack batteryPack)
		{
			var actuals = batteryPack.Actuals;

			Console.WriteLine("Current battery conditions:");
			Console.WriteLine("Voltage:             {0} V ({1})", 
				actuals.Voltage, 
				batteryPack.SubElements.Select((c, i) => string.Format("{0}: {1} V", i, c.Actuals.Voltage)).Join(", "));
			Console.WriteLine("Current:                  {0} mA", actuals.ActualCurrent * 1000f);
			Console.WriteLine("Average current:          {0} mA", actuals.AverageCurrent * 1000f);
			Console.WriteLine("Temperature:              {0:f2} °C", actuals.Temperature - 273.15f);
			Console.WriteLine("Remaining capacity:       {0:N0} mAh", actuals.RemainingCapacity * 1000f);
			Console.WriteLine("Absolute StateOfCharge:   {0} %", actuals.AbsoluteStateOfCharge * 100f);
			Console.WriteLine("Relative StateOfCharge:   {0} %", actuals.RelativeStateOfCharge * 100f);
			Console.WriteLine("Actual run time:          {0}", actuals.ActualRunTime);
			Console.WriteLine("Average run time:         {0}", actuals.AverageRunTime);
			//Console.WriteLine("Charging voltage:         {0}", actuals.ChargingVoltage);
			//Console.WriteLine("Charging current:         {0}", actuals.ChargingCurrent);
			Console.WriteLine();
		}

		private static void ReportAlarmSettings(Battery battery)
		{
			//Console.WriteLine("Remaining capacity alarm: {0}", (battery.Health.RemainingCapacityAlarm > 0 ? String.Format("{0:N0} mAh", battery.Status.RemainingCapacityAlarm * 1000) : DisabledText));
			//Console.WriteLine("Remaining time alarm:     {0}", (battery.Health.RemainingTimeAlarm > TimeSpan.Zero ? battery.Status.RemainingTimeAlarm.ToString() : DisabledText));
		}

		private static void TestAllCommands(SMBusInterface connection, uint address)
		{
			connection.QuickCommand(address);
			//connection.SendByte(address, 0x79);
			connection.WriteByteCommand(address, 0x01, 0x79);
			connection.WriteWordCommand(address, 0x02, 0x4567);
			connection.WriteBlockCommand(address, 0x03, new byte[] { 0x12, 0x34, 0x56, 0x78 });

			//byte tmpData = connection.ReceiveByte(address);
			byte byteData = connection.ReadByteCommand(address, 0x04).Result;
			ushort wordData = connection.ReadWordCommand(address, 0x05).Result;
			byte[] blockData = connection.ReadBlockCommand(address, 0x06, 4).Result;
		}

		private static void TestI2C()
		{
			NativeMethods.Init_libMPSSE();

			FTDI.FT_STATUS status;

			uint channelCount = 0;
			status = NativeMethods_I2C.I2C_GetNumChannels(out channelCount);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Unable to find number of I2C channels. (Status: " + status + ")");
			}

			var infoNode = new NativeMethods.FT_DEVICE_LIST_INFO_NODE();
			status = NativeMethods_I2C.I2C_GetChannelInfo(0, infoNode);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Unable to open I2C channel. (Status: " + status + ")");
			}

			IntPtr i2cHandle;

			status = NativeMethods_I2C.I2C_OpenChannel(0, out i2cHandle);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Unable to open I2C channel. (Status: " + status + ")");
			}

			var config = new NativeMethods_I2C.ChannelConfig(NativeMethods_I2C.ClockRate.Standard, 1, NativeMethods_I2C.ConfigOptions.None);
			status = NativeMethods_I2C.I2C_InitChannel(i2cHandle, config);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Unable to initialize I2C channel. (Status: " + status + ")");
			}

			byte gpioState;
			status = NativeMethods.FT_ReadGPIO(i2cHandle, out gpioState);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Unable to open I2C channel. (Status: " + status + ")");
			}

			status = NativeMethods_I2C.I2C_CloseChannel(i2cHandle);
			i2cHandle = IntPtr.Zero;
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Unable to close I2C channel. (Status: " + status + ")");
			}

			NativeMethods.Cleanup_libMPSSE();

			return;
		}

		private static void TestSPI()
		{
			NativeMethods.Init_libMPSSE();

			FTDI.FT_STATUS status;

			uint channelCount = 0;
			status = SPI.NativeMethods_SPI.SPI_GetNumChannels(out channelCount);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Unable to find number of SPI channels. (Status: " + status + ")");
			}

			var infoNode = new NativeMethods.FT_DEVICE_LIST_INFO_NODE();
			status = SPI.NativeMethods_SPI.SPI_GetChannelInfo(0, infoNode);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Unable to open SPI channel. (Status: " + status + ")");
			}

			IntPtr spiHandle;

			status = SPI.NativeMethods_SPI.SPI_OpenChannel(0, out spiHandle);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Unable to open SPI channel. (Status: " + status + ")");
			}

			var configOptions = SPI.NativeMethods_SPI.ConfigOptions.SPI_CONFIG_OPTION_MODE3 |
									SPI.NativeMethods_SPI.ConfigOptions.SPI_CONFIG_OPTION_CS_DBUS3 |
									SPI.NativeMethods_SPI.ConfigOptions.SPI_CONFIG_OPTION_CS_ACTIVELOW;
			var config = new SPI.NativeMethods_SPI.ChannelConfig(100000, 1, configOptions, 0);
			status = SPI.NativeMethods_SPI.SPI_InitChannel(spiHandle, config);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Unable to initialize SPI channel. (Status: " + status + ")");
			}

			var transferOptions = SPI.NativeMethods_SPI.TransferOptions.SPI_TRANSFER_OPTIONS_SIZE_IN_BYTES |
			                      SPI.NativeMethods_SPI.TransferOptions.SPI_TRANSFER_OPTIONS_CHIPSELECT_ENABLE |
			                      SPI.NativeMethods_SPI.TransferOptions.SPI_TRANSFER_OPTIONS_CHIPSELECT_DISABLE;
			var buffer = new byte[] { 0xFF};
			uint bytesTransferred = 0;
			status = SPI.NativeMethods_SPI.SPI_Write(spiHandle, buffer, 1, out bytesTransferred, transferOptions);

			Thread.Sleep(1);



			//byte gpioState;
			//status = NativeMethods.FT_ReadGPIO(spiHandle, out gpioState);
			//if (status != FTDI.FT_STATUS.FT_OK)
			//{
			//	throw new InvalidOperationException("Unable to open SPI channel. (Status: " + status + ")");
			//}

			status = SPI.NativeMethods_SPI.SPI_CloseChannel(spiHandle);
			spiHandle = IntPtr.Zero;
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Unable to close SPI channel. (Status: " + status + ")");
			}

			NativeMethods.Cleanup_libMPSSE();

			return;
		}

		private static void TestLtc6804()
		{
			// Discover bus devices
			IEnumerable<IDiscoverDevices> dicoveryServices = GetDiscoveryServices();
			var discoveryTask = Task.WhenAll(dicoveryServices.Select(x => x.GetConnectors()));
			var allDevices = discoveryTask.Result.SelectMany(x => x);

			var connector = allDevices.OfType<SPI.Device>().FirstOrDefault();
			if (connector == null)
			{
				Console.WriteLine("No SPI device found.");
				return;
			}

			Console.WriteLine("Connecting to SPI device '{0}' of type '{1}'.", connector.Name, connector.Type);

			var connection = connector.Connect().Result;
			var adapter = new LTC6804.BatteryAdapter(connection as ICommunicateToBus);

			adapter.RecognizeBattery().Wait();
			adapter.UpdateReadings().Wait();
			adapter.PerformSelfTest().Wait();

			var battery = adapter.Pack;
		}

		private static IEnumerable<uint> DiscoverDevices(SMBusInterface connection)
		{
			for (uint addr = 1; addr < 128; addr++)
			{
				try
				{
					connection.ReadWordCommand(addr, 0x09);
				}
				catch (InvalidOperationException)
				{
					continue;
				}

				yield return addr;
			}
		}

		private static IEnumerable<IDiscoverDevices> GetDiscoveryServices()
		{
			return new IDiscoverDevices[] { new MpsseDiscoveryService() };
		}
	}
}
