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
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using FoxHollow.FHM.Core;
using FoxHollow.FHM.Core.Operations;

namespace FoxHollow.FHM
{
    internal class MainService
    {
        private IServiceProvider _serviceProvider;

        public MainService(IServiceProvider provider)
        {
            _serviceProvider = provider;
        }

        public async Task RunAsync()
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<MainService>>();

            AppInfo.InitPlatform();

            var cts = new CancellationTokenSource();

            // var organizer = new OrganizeRawMediaOperation(_serviceProvider);
            // await organizer.StartAsync(cts.Token);

            var prepareTiff = new ProcessPhotosOperation(_serviceProvider);
            await prepareTiff.StartAsync(cts.Token);
        }
    }
}