using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace TTStreamer.Data
{
    public class GiftData
    {
        [JsonPropertyName("giftId")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("image")]
        public string Image { get; set; }
    }
}