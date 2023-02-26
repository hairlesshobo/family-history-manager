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