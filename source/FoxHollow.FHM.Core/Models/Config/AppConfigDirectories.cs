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

namespace FoxHollow.FHM.Core.Models;

public class AppConfigDirectories
{
    public string VideoRoot { get; set; }
    public AppConfigDirectory Raw { get; set; }
    public AppConfigDirectory WebFootage { get; set; }
    public AppConfigDirectory FinalFootage { get; set; }
}
