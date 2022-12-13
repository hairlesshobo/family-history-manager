using System;
using System.IO;
using System.Text.Json;
using FoxHollow.FHM.Core.Classes;

namespace FoxHollow.FHM
{
    public static class SysInfo
    {
        public static bool Initialized { get; private set; }
        public static string ExecutionRoot { get; private set; }
        public static Config Config { get; private set; }

        static SysInfo()
        {
            ExecutionRoot = AppContext.BaseDirectory;
        }

        public static void InitPlatform()
        {
            if (!Initialized)
            {
                // using (FileSteam stream = File.Open())
                // Config = JsonSerializer.Deserialize<Config>()
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