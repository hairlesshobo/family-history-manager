using System;

namespace FoxHollow.FHM.Core.Models;

public class AppConfigPhotosTiff
{
    public bool RequireCompression { get; set; }
    public AppConfigDirectory Directories { get; set; } = new AppConfigDirectory();
}