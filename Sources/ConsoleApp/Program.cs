using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FTD2XX_NET;
using ImpruvIT;

using ImpruvIT.BatteryMonitor.Domain.Battery;
using ImpruvIT.BatteryMonitor.Hardware;
using ImpruvIT.BatteryMonitor.Hardware.Ftdi;
using ImpruvIT.BatteryMonitor.Protocols;
using ImpruvIT.BatteryMonitor.Protocols.SMBus;
using NativeMethods = ImpruvIT.BatteryMonitor.Hardware.Ftdi.NativeMethods;
using I2C = ImpruvIT.BatteryMonitor.Hardware.Ftdi.I2C;
using SPI = ImpruvIT.BatteryMonitor.Hardware.Ftdi.SPI;
using LTC6804 = ImpruvIT.BatteryMonitor.Protocols.LinearTechnology.LTC6804;

namespace ImpruvIt.BatteryMonitor.ConsoleApp
{
	class Program
	{
		public static string DisabledText = "Disabled";

		static async Task Main()
		{
			log4net.Config.XmlConfigurator.Configure();

            //var protocol = new LTC6804.LTC6804_1Interface(new SPI.Connection(), 1);
            //protocol.ReadRegister(LTC6804.CommandId.ReadStatusRegisterB, 6).Wait();

            //TestI2C();
            //TestSPI();
            //TestLtc6804();

            // Start monitor thread;
            await MonitorBattery();
		}

		private static async Task MonitorBattery()
		{
			// Discover bus devices
			IEnumerable<IDiscoverDevices> dicoveryServices = GetDiscoveryServices();
			var discoveryTask = Task.WhenAll(dicoveryServices.Select(x => x.GetConnectors()));
			var allConnectors = discoveryTask.Result.SelectMany(x => x);

			var connector = allConnectors.OfType<I2C.Device>().FirstOrDefault();
			if (connector == null)
			{
				Console.WriteLine("No SMBus connector found.");
				return;
			}

			Console.WriteLine("Connecting to SMBus '{0}' of type '{1}'.", connector.Name, connector.Type);

			var connection = await connector.Connect();
			try
			{
				//uint address = (await DiscoverDevices(new SMBusInterface((ICommunicateToAddressableBus)connection))).First();
                //uint address = 0x2A;
                byte address = 0x0B;

                var batteryAdapter = new BatteryAdapter(new SMBusInterface((ICommunicateToAddressableBus)connection), address);
				await batteryAdapter.RecognizeBattery();
				var batteryPack = batteryAdapter.Pack;

				Console.WriteLine("Battery found at address {0}:", address);
				var productDefinition = batteryPack.ProductDefinition();
				var designParameters = batteryPack.DesignParameters();
				Console.WriteLine("Manufacturer:             {0}", productDefinition.Manufacturer);
				Console.WriteLine("Product:                  {0}", productDefinition.Product);
				Console.WriteLine("Chemistry:                {0}", productDefinition.Chemistry);
				Console.WriteLine("Manufacture date:         {0}", productDefinition.ManufactureDate.ToShortDateString());
				Console.WriteLine("Serial number:            {0}", productDefinition.SerialNumber);
				//Console.WriteLine("Specification version:    {0}", battery.Information.SpecificationVersion.ToString(2));
				Console.WriteLine("Cell count:               {0} cells", batteryPack.ElementCount);
				Console.WriteLine("Nominal voltage:          {0} V", designParameters.NominalVoltage);
				Console.WriteLine("DesignedDischargeCurrent: {0} A", designParameters.DesignedDischargeCurrent);
				Console.WriteLine("MaxDischargeCurrent:      {0} A", designParameters.MaxDischargeCurrent);
				Console.WriteLine("Designed capacity:        {0:N0} mAh", designParameters.DesignedCapacity * 1000);
				//Console.WriteLine("Voltage scale:            {0}x", battery.Information.VoltageScale);
				//Console.WriteLine("Current scale:            {0}x", battery.Information.CurrentScale);
				Console.WriteLine();

                int counter = 0;
                while(true)
                {
                    try
                    {
                        if (counter == 0)
                            await batteryAdapter.ReadHealth();
                        counter = (counter + 1) % 5;
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Updating health failed!");
                    }

                    try
                    {
                        await batteryAdapter.ReadActuals();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Updating actuals failed!");
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
				
				var health = batteryPack.Health();
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

                //var subscription = batteryAdapter.SubscribeToUpdates(PrintActuals, UpdateFrequency.Normal);

				Console.ReadLine();

				//subscription.Unsubscribe();
			}
			finally
			{
				await connection.Disconnect();
			}
		}

		private static void PrintActuals(Pack pack)
		{
			var actuals = pack.Actuals();

			Console.WriteLine("Current battery conditions:");
			Console.WriteLine("Voltage:             {0} V ({1})", 
				actuals.Voltage, 
				pack.SubElements.Select((c, i) => string.Format("{0}: {1} V", i, c.Actuals().Voltage)).Join(", "));
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

		private static void ReportAlarmSettings(Pack battery)
		{
			//Console.WriteLine("Remaining capacity alarm: {0}", (battery.Health.RemainingCapacityAlarm > 0 ? String.Format("{0:N0} mAh", battery.Status.RemainingCapacityAlarm * 1000) : DisabledText));
			//Console.WriteLine("Remaining time alarm:     {0}", (battery.Health.RemainingTimeAlarm > TimeSpan.Zero ? battery.Status.RemainingTimeAlarm.ToString() : DisabledText));
		}

		private static void TestAllCommands(SMBusInterface connection, byte address)
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
			status = I2C.NativeMethods_I2C.I2C_GetNumChannels(out channelCount);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Unable to find number of I2C channels. (Status: " + status + ")");
			}

			var infoNode = new NativeMethods.FT_DEVICE_LIST_INFO_NODE();
			status = I2C.NativeMethods_I2C.I2C_GetChannelInfo(0, infoNode);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Unable to open I2C channel. (Status: " + status + ")");
			}

			IntPtr i2cHandle;

			status = I2C.NativeMethods_I2C.I2C_OpenChannel(0, out i2cHandle);
			if (status != FTDI.FT_STATUS.FT_OK)
			{
				throw new InvalidOperationException("Unable to open I2C channel. (Status: " + status + ")");
			}

			var config = new I2C.NativeMethods_I2C.ChannelConfig(I2C.NativeMethods_I2C.ClockRate.Standard, 1, I2C.NativeMethods_I2C.ConfigOptions.None);
			status = I2C.NativeMethods_I2C.I2C_InitChannel(i2cHandle, config);
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

			status = I2C.NativeMethods_I2C.I2C_CloseChannel(i2cHandle);
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

			var connection = connector.Connect().Result as ICommunicateToBus;
			var adapter = new LTC6804.BatteryAdapter(connection);

			while (adapter.ChainLength == 0)
			{
				//connection.Send(new byte[] {0xAA, 0xF0});
				adapter.RecognizeBattery().Wait();
			}
			
			adapter.UpdateReadings().Wait();
			adapter.PerformSelfTest().Wait();

			var battery = adapter.Pack;
		}

		private static async Task<IEnumerable<uint>> DiscoverDevices(SMBusInterface connection)
		{
            var addresses = new List<uint>();

			for (byte addr = 11; addr < 12; addr++)
			{
				try
				{
					var response = await connection.ReadWordCommand(addr, 0x09);
                    if (response == 0)
                        continue;
				}
				catch (AggregateException ex)
				{
                    if (ex.InnerException is InvalidOperationException)
					    continue;

                    throw;
				}

                addresses.Add(addr);
			}

            return addresses;
		}

		private static IEnumerable<IDiscoverDevices> GetDiscoveryServices()
		{
			return new IDiscoverDevices[] { new MpsseDiscoveryService() };
		}
	}
}
