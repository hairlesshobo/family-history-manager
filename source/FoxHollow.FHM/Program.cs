// See https://aka.ms/new-console-template for more information
using System;
using System.Threading;
using FoxHollow.FHM.Core;
using FoxHollow.FHM.Core.Operations;

AppInfo.InitPlatform();

// TODO: implement dependency injection
// https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection-usage

var cts = new CancellationTokenSource();

var generator = new OrganizeRawMediaOperation();
await generator.StartAsync(cts.Token);

