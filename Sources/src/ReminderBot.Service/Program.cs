using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ReminderBot.DataAccess.Extensions;
using ReminderBot.Models.Models;
using ReminderBot.Service.Extensions;

namespace ReminderBot.Service
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var isService = !(Debugger.IsAttached || args.Contains("console"));

            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddJsonFile($"appsettings.json", optional: false, reloadOnChange: false);
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: false);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddServices();
                    services.AddDbContexts();
                    services.AddHandlers();
                    services.AddCommands();
                    services.AddHostedServices();
                    services.AddOption<BotOptions>(hostContext.Configuration);
                    services.AddLocalization(options => options.ResourcesPath = "Resources");
                })
                .UseDefaultServiceProvider(options => options.ValidateOnBuild = true)
                .ConfigureLogging((hostingContext, config) =>
                {
                    var loggingConfig = hostingContext.Configuration.GetSection("Logging");

                    config.AddConfiguration(loggingConfig);
                    config.ClearProviders();
                    config.AddConsole();
                });

            if (isService)
            {
                await builder.UseSystemd().RunConsoleAsync();
            }
            else
            {
                Console.WriteLine("Press CTRL+C to stop the application");
                await builder.RunConsoleAsync();
            }
        }
    }
}