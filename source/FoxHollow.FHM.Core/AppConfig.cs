using System;
using FoxHollow.FHM.Core.Classes;

namespace FoxHollow.FHM.Core
{
    public static class AppConfig
    {
        public static bool Initialized { get; private set; }
        public static Config Config { get; private set; }

        public static void InitPlatform()
        {
            if (!AppConfig.Initialized)
            {
                // _config = ConfigUtils.ReadConfig(out _configErrors);

                // _directories = new SystemDirectories();
                // _directories.Bin = PathUtils.CleanPath(AppContext.BaseDirectory);
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

                AppConfig.Initialized = true;
            }
        }
        /// <summary>
        ///     Class that describes the system directories
        /// </summary>
        public class SystemDirectories
        {
            /// <summary>
            ///     Full path to the directory of the executable that is currently running
            /// </summary>
            public string Bin { get; internal protected set; }

            /// <summary>
            ///     Full path to the archive index directory
            /// </summary>
            public string Index { get; internal protected set; }

            /// <summary>
            ///     Full path to the archive json index directory
            /// </summary>
            public string JSON { get; internal protected set; }

            /// <summary>
            ///     Full path to the staging directory to use when archiving to disc
            /// </summary>
            public string DiscStaging { get; internal protected set; }

            /// <summary>
            ///     Full path to the directory where ISO files will be created
            /// </summary>
            public string ISO { get; internal protected set; }
        }
    }
}