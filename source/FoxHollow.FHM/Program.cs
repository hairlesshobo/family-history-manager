﻿//==========================================================================
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
using Avalonia;
using Avalonia.ReactiveUI;
using FoxHollow.FHM.Core.Models;
using FoxHollow.FHM.Shared;
using FoxHollow.FHM.Shared.Services;
using FoxHollow.FHM.Shared.Storage;
using FoxHollow.FHM.Shared.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace FoxHollow.FHM;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        var serviceProvider = ConfigureServices();

        return AppBuilder.Configure<App>(() => new App(serviceProvider))
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
    }

    private static IServiceProvider ConfigureServices()
    {
        var config = Configure();

        var configModel = new AppConfig();

        config.Bind(configModel);

        var collection = new ServiceCollection();
        collection.AddSingleton<IConfiguration>(config);
        collection.AddSingleton<AppConfig>(configModel);
        collection.AddFhmStartupServices();
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
            logging.AddEventLogger();
        });
        collection.AddFhmServices();
        // collection.AddScoped<MainService>();

        var resolver = new MicrosoftDependencyResolver(collection);
        Locator.SetLocator(resolver);

        var serviceProvider = collection.BuildServiceProvider();

        ServiceExtensions.InitFhmPlatform(serviceProvider);

        return serviceProvider;
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

    // private static void DisposeServices()
    // {
    //     if (_serviceProvider != null && _serviceProvider is IDisposable)
    //         ((IDisposable)_serviceProvider).Dispose();
    // }
}
