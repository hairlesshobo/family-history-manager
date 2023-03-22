//==========================================================================
//  Family History Manager - https://code.foxhollow.cc/fhm/
//
//  A cross platform tool to help organize and preserve all types
//  of family history
//==========================================================================
//  Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//==========================================================================

using System;
using System.Threading.Tasks;
using FoxHollow.FHM.Core.Models;
using FoxHollow.FHM.Shared;
using FoxHollow.FHM.Shared.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

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