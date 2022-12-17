using System;
using FoxHollow.FHM.Core.Models;
using FoxHollow.FHM.Core.Utilities;

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

                // _directories = new SystemDirectories();
                // new name: platform
                // _directories.Index = PathUtils.ResolveRelativePath(Path.Combine(_directories.Bin, "../"));
                // _directories.JSON = PathUtils.CleanPathCombine(_directories.Index, "json");
                // _directories.ISO = PathUtils.CleanPathCombine(_directories.Index, "../iso");
                // _directories.DiscStaging = (
                //       !String.IsNullOrWhiteSpace(Config.Disc.StagingDir) 
                //     ? PathUtils.ResolveRelativePath(Path.Combine(_directories.Index, Config.Disc.StagingDir)) 
                //     : PathUtils.CleanPath(Path.Combine(Path.GetTempPath(), Path.GetTempFileName()))
                // );

                // _isOpticalDrivePresent = OpticalDriveUtils.GetDriveNames().Any();
                // _isReadonlyFilesystem = TestForReadonlyFs();
                // _isTapeDrivePresent = TapeUtilsNew.IsTapeDrivePresent();

                AppInfo.Initialized = true;
            }
        }
    }
}