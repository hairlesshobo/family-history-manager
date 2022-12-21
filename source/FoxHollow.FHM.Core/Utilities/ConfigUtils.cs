using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using FoxHollow.FHM.Core.Models;
using FoxHollow.FHM.Shared;
using FoxHollow.FHM.Shared.Models.Video;
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

        public static IReadOnlyList<CameraProfile> LoadCameraProfiles()
        {
            var profiles = new List<CameraProfile>();
            var proflieDirPath = Path.Join(SysInfo.ConfigRoot, "profiles");
            var filesPaths = Directory.GetFiles(proflieDirPath, "*.json");

            foreach (var filePath in filesPaths)
            {
                using (var fileHandle = File.OpenRead(filePath))
                {
                    var camProfile = JsonSerializer.Deserialize<CameraProfile>(fileHandle, Static.DefaultJso);
                    profiles.Add(camProfile);
                }
            }

            return (IReadOnlyList<CameraProfile>)profiles;
        }
    }
}