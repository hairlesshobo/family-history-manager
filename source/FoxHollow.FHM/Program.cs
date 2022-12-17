// See https://aka.ms/new-console-template for more information
using System;
using System.Threading;
using FoxHollow.FHM.Core;
using FoxHollow.FHM.Core.Operations;

AppInfo.InitPlatform();

var cts = new CancellationTokenSource();

var generator = new GenerateRawMediaSidecarOperation();
await generator.StartAsync(cts.Token);

