namespace FoxHollow.FHM.Core.Models
{
    public class AppConfigDirectory
    {
        public string Root { get; set; }
        public string[] Include { get; set; }
        public string[] Exclude { get; set; }
        public string[] Extensions { get; set; }
    }
}