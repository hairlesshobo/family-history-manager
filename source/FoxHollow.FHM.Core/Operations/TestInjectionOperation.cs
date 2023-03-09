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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Core.Models;
using FoxHollow.FHM.Shared.Classes;
using FoxHollow.FHM.Shared.Models;
using FoxHollow.FHM.Shared.Utilities;
using FoxHollow.FHM.Shared.Utilities.Serialization;
using ImageMagick;
using ImageMagick.Formats;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;

namespace FoxHollow.FHM.Core.Operations;

public class TestInjectionOperation
{
    private IServiceProvider _services;
    private ILogger _logger;
    private AppConfig _config;

    public TestInjectionOperation(IServiceProvider provider)
    {
        _services = provider;

        _logger = provider.GetRequiredService<ILogger<ProcessPhotosOperation>>();
        _config = provider.GetRequiredService<AppConfig>();
    }

    public async Task StartAsync(CancellationToken ctk)
    {
        _logger.LogInformation("meow1");
        await Task.Delay(1000);
        _logger.LogInformation("meow2");
        await Task.Delay(1000);
        _logger.LogInformation("meow3");
        await Task.Delay(1000);
        _logger.LogInformation("meow4");
        await Task.Delay(1000);
    }

}
