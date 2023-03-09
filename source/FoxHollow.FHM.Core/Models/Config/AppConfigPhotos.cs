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

namespace FoxHollow.FHM.Core.Models;

public class AppConfigPhotos
{
    public int ThumbnailSize { get; set; }
    public string ThumbnailExtension { get; set; }
    public int PreviewSize { get; set; }
    public string PreviewExtension { get; set; }
    public AppConfigPhotosTiff Tiff { get; set; } = new AppConfigPhotosTiff();
}
