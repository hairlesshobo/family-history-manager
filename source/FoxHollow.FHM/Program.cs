// See https://aka.ms/new-console-template for more information
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FoxHollow.FHM.Shared;
using FoxHollow.FHM.Shared.Utilities;
using FoxHollow.FHM.Shared.Classes;
using System.DirectoryServices.ActiveDirectory;
using Microsoft.Extensions.Logging.Console;

namespace FoxHollow.FHM
{
    public class Program
    {
        private static IServiceProvider _serviceProvider;
        
        private static async Task Main()
        {
            RegisterServices();

            // Call main entry point of the application
            var service = _serviceProvider.GetService<MainService>();

            await service.RunAsync();

            DisposeServices();
        }

        private static void RegisterServices()
        {
            var config = Configure();

            var collection = new ServiceCollection();
            collection.AddSingleton<IConfiguration>(config);
            collection.AddLogging(logging => 
            {
                logging.AddConfiguration(config.GetSection("Logging"));
                logging.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.ColorBehavior = LoggerColorBehavior.Enabled;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                });
            });
            collection.AddFhmServices();
            collection.AddScoped<MainService>();

            _serviceProvider = collection.BuildServiceProvider();
        }

        private static IConfiguration Configure()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(SysInfo.ConfigRoot)
                .AddJsonFile("logging.json", optional: false, reloadOnChange: false)
                .AddJsonFile("manager.json", optional: false, reloadOnChange: false)
                .Build();

            return config;
        }

        private static void DisposeServices()
        {
            if (_serviceProvider != null && _serviceProvider is IDisposable)
                ((IDisposable)_serviceProvider).Dispose();
        }
    }
}

