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

            var prepareTiff = new ProcessPhotosTiffOperation(_serviceProvider);
            await prepareTiff.StartAsync(cts.Token);
        }
    }
}