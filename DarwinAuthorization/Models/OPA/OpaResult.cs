using System.Text.Json.Serialization;

namespace DarwinAuthorization.Models.OPA
{
    public class OpaResult
    {
        [JsonPropertyName("result")]
        public Result Result { get; set; }
    }
}
