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

using Avalonia;
using Avalonia.ReactiveUI;
using FoxHollow.FHM.Core.Models;
using FoxHollow.FHM.Shared;
using FoxHollow.FHM.Shared.Services;
using FoxHollow.FHM.Shared.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;
using System;

namespace FoxHollow.FHM.UI;

class Program
{
    public static IServiceProvider Services { get; private set; }
    // private static IServiceProvider _serviceProvider;

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
        ConfigureServices();

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace()
            .UseReactiveUI();
    }

    private static void ConfigureServices()
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

        Services = collection.BuildServiceProvider();
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
