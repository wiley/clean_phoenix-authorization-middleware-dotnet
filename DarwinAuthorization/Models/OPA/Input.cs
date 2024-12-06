using Microsoft.AspNetCore.Http;
using System.Text.Json.Serialization;

namespace DarwinAuthorization.Models.OPA
{
    public class Input
    {
        [JsonPropertyName("path")]
        public string[] Path { get; set; }
        [JsonPropertyName("method")]
        public string Method { get; set; }
        [JsonPropertyName("body")]
        public string? Body { get; set; }
        [JsonPropertyName("query")]
        public string Query { get; set; }
        [JsonPropertyName("jwt")]
        public string Jwt { get; set; }
        [JsonPropertyName("headers")]
        public string Headers { get; set; }
    }
}
