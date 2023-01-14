using System;
using System.Collections.Generic;
using FoxHollow.FHM.Core.Models;
using FoxHollow.FHM.Core.Utilities;
using FoxHollow.FHM.Shared.Models.Video;

namespace FoxHollow.FHM.Core
{
    public static class AppInfo
    {
        public static bool Initialized { get; private set; } = false;
        public static AppConfig Config { get; private set; }

        public static void InitPlatform()
        {
            if (!AppInfo.Initialized)
            {
                AppInfo.Config = ConfigUtils.LoadConfig();

                AppInfo.Initialized = true;
            }
        }
    }
}