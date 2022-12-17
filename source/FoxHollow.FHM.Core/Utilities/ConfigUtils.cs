using System;
using System.IO;
using System.Text.Json;
using FoxHollow.FHM.Core.Models;
using FoxHollow.FHM.Shared;
using FoxHollow.FHM.Shared.Utilities;

namespace FoxHollow.FHM.Core.Utilities
{
    public static class ConfigUtils
    {
        public static AppConfig LoadConfig()
        {
            string configFile = Path.Join(SysInfo.ConfigRoot, "manager.json");

            return HelpersNew.ReadJsonFileAsync<AppConfig>(configFile).Result;
        }
    }
}