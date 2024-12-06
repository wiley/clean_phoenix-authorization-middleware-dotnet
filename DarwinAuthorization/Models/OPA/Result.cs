using System.Text.Json.Serialization;

namespace DarwinAuthorization.Models.OPA
{
    public class Result
    {
        [JsonPropertyName("allow")]
        public bool allow { get; set; }
    }
}