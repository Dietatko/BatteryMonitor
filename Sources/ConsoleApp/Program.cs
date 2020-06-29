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
using ImpruvIT.BatteryMonitor.Protocols.SMBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;
using NativeMethods = ImpruvIT.BatteryMonitor.Hardware.Ftdi.NativeMethods;
using I2C = ImpruvIT.BatteryMonitor.Hardware.Ftdi.I2C;
using SPI = ImpruvIT.BatteryMonitor.Hardware.Ftdi.SPI;
using LTC6804 = ImpruvIT.BatteryMonitor.Protocols.LinearTechnology.LTC6804;

namespace ImpruvIt.BatteryMonitor.ConsoleApp
{
	class Program
	{
		public static string DisabledText = "Disabled";

		static async Task Main(string[] args)
		{
			await CreateHostBuilder(args).Build().RunAsync();
		}
		
		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					services.AddHostedService<Worker>();
				});

		
	}
}
