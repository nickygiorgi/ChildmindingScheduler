using System.Text.Json.Serialization;

namespace ChildmindingScheduler
{
    internal class Session
    {
        public required TimeOnly Start { get; set; }

        public required TimeOnly End { get; set; }

        [JsonInclude]
        public double SessionRate { get; set; }

        [JsonInclude]
        public double HourlyRate { get; set; }
    }
}
