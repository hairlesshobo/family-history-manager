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
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Shared.Classes;
using FoxHollow.FHM.Shared.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Cli;

internal class MainService
{
    private IServiceProvider _services;
    private ILogger _logger;

    public MainService(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = _services.GetRequiredService<ILogger<MainService>>();
    }

    public async Task RunAsync()
    {

        var cts = new CancellationTokenSource();

        // TODO: Move to actual test suite
        // RunDateTests();

        var spConfig = new ProviderConfigCollection();
        spConfig.Add(new Config)
        var sp = new LocalStorageProvider(_services);
        await sp.ConnectAsync();

        await foreach (var entry in sp.RootDirectory.ListDirectoryAsync())
        {
            _logger.LogInformation(entry.Path);
        }

        // var organizer = new OrganizeRawMediaOperation(_serviceProvider);
        // await organizer.StartAsync(cts.Token);

        // var prepareTiff = new ProcessPhotosOperation(_serviceProvider);
        // await prepareTiff.StartAsync(cts.Token);
    }

    public void RunDateTests()
    {

        Console.WriteLine(TestDate("about 1983"));
        Console.WriteLine(TestDate("abt  1967"));
        Console.WriteLine(TestDate("abt. 1922"));
        Console.WriteLine(TestDate("abt. 1922-1924"));
        Console.WriteLine(TestDate("1941"));
        Console.WriteLine(TestDate("1935-1938"));
        Console.WriteLine(TestDate("spring 1945"));

        Console.WriteLine(TestDate("2022-02-21"));
        Console.WriteLine(TestDate("spring 1996"));
        Console.WriteLine(TestDate("~2002"));
        Console.WriteLine(TestDate("about 1985"));
        Console.WriteLine(TestDate("abt. 1960"));
        Console.WriteLine(TestDate("1930s"));

        Console.WriteLine(TestDate("Dec 2022"));
        // Console.WriteLine(TestDate("March 1974 - July 1974"));
        // Console.WriteLine(TestDate("Mar 1974 - Jul 1974"));


        Console.WriteLine(TestDate("4 Dec 2011"));
        Console.WriteLine(TestDate("26 Nov 1989"));
        Console.WriteLine(TestDate("Nov. 1989"));
        Console.WriteLine(TestDate("1989-11"));
        Console.WriteLine(TestDate("1989-11-26"));
        Console.WriteLine(TestDate("between 1974-1977"));

    }

    private string TestDate(string fDate)
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($" input: {fDate}");
        var data = new FlexibleDate(fDate);
        
        sb.AppendLine("  json: " + JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = false }));
        sb.AppendLine("output: " + data.ToString());

        return sb.ToString();
    }
}
