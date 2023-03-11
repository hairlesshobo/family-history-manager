/**
 *  Family History Manager - https://code.foxhollow.cc/fhm/
 *
 *  A cross platform tool to help organize and preserve all types
 *  of family history
 * 
 *  Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
 *
 *  This Source Code Form is subject to the terms of the Mozilla Public
 *  License, v. 2.0. If a copy of the MPL was not distributed with this
 *  file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

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
using FoxHollow.FHM.Core.Models;

namespace FoxHollow.FHM.Cli;

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

        var configModel = new AppConfig();

        config.Bind(configModel);

        var collection = new ServiceCollection();
        collection.AddSingleton<IConfiguration>(config);
        collection.AddSingleton<AppConfig>(configModel);
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