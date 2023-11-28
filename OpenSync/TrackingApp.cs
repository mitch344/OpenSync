using Newtonsoft.Json;

namespace OpenSync
{
    internal class TrackingApp
    {
        public string ProcessToTrack { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }

        [JsonIgnore]
        public bool IsRunning { get; set; } = false;
    }
}
