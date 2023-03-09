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