using System.Collections.Generic;

namespace HaveYouReadThis
{
    public class ProgressionInfoPlayerEntry
    {
        public string PlayerDisplayName { get; set; }
        public Dictionary<string, ProgressionInfo> Progressions { get; set; }
    }
}