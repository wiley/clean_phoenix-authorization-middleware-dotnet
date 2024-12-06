using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarwinAuthorization.UnitTests.Util
{
    public class OPARequestContent
    {
        [JsonProperty("input")]
        public OPARequestContentInput Input { get; set; }
    }

    public class OPARequestContentInput
    {
        [JsonProperty("path")]
        public List<string> Path { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("jwt")]
        public string JWT { get; set; }

        [JsonProperty("headers")]
        public string Headers { get; set; }
    }
}