namespace FoxHollow.FHM.Core.Models
{
    public class AppConfigDirectory
    {
        public string Root { get; set; }
        public string[] Include { get; set; } = new string[] { };
        public string[] Exclude { get; set; } = new string[] { };
        public string[] Extensions { get; set; } = new string[] { };
    }
}