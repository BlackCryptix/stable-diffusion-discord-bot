
using System.Text.Json.Serialization;


namespace SDDiscordBot.Models
{
    public struct SDProgress
    {
        [JsonPropertyName("progress")]
        public float Progress { get; set; }

        [JsonPropertyName("eta_relative")]
        public double? EtaRelative { get; set; }

        [JsonPropertyName("state")]
        public object? State { get; set; }

        [JsonPropertyName("current_image")]
        public string CurrentImage { get; set; }

        [JsonPropertyName("textinfo")]
        public string? Textinfo { get; set; }
    }
}
