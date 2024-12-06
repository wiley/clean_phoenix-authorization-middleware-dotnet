using DarwinAuthorization.Interfaces;
using DarwinAuthorization.Models;
using DarwinAuthorization.Models.OPA;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Net.Http.Headers;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.IdentityModel.Protocols;

namespace DarwinAuthorization.Services
{
    public class OPAService : IOPAService
    {
        private HttpClient _httpClient;
        private readonly ILogger<OPAService> _logger;

        public OPAService(ILogger<OPAService> logger, HttpClient client)
        {
            _logger = logger;
            _httpClient = client;
        }
        public async Task<bool> RedirectOPA(HttpRequest request, DarwinAuthorizationConfig config)
        {
            try
            {
                _httpClient.BaseAddress = new Uri(config.OpaBaseUrl);

                string jwt = request.Headers.Authorization.ToString().Replace("Bearer ", "");
                string header = JsonConvert.SerializeObject(request.Headers);
                string query = GetSerializedQueryParameters(request);
                string body = GetSerializedBody(request);
                string[] path = GetPath(request);
                string resourceName = path[1].ToString();

                Data data = new Data
                {
                    Input = new Input
                    {
                        Path = path,
                        Method = request.Method,
                        Body = body,
                        Query = query,
                        Jwt = jwt.ToString().Replace("Bearer ", ""),
                        Headers = header
                    }
                };

                var serializerSettings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };
                var jsonBody = JsonConvert.SerializeObject(data, serializerSettings);
                byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                var content = new ByteArrayContent(messageBytes);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await _httpClient.PostAsync($"v1/data/darwin/resources/{resourceName}", content);
                var opaResult = JsonConvert.DeserializeObject<OpaResult>(response.Content.ReadAsStringAsync().Result);

                if (opaResult is not null)
                {
                    if (opaResult.Result is null)
                    {
                        return false;
                    }
                    if (opaResult.Result.allow)
                    {
                        return true;
                    }
                    _logger.LogInformation("Permission denied.");
                    return false;
                }
                _logger.LogInformation("Permission denied.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error on call OPA Service, please check message: {ex.InnerException} - {ex.Message} - {ex.StackTrace} ");
                throw;
            }
        }

        private string[] GetPath(HttpRequest request)
        {
            List<string> listPath = new List<string>();
            listPath = request.Path.Value.Split('/').ToList();
            var path = listPath.Where(x => !x.Equals("api") && !x.Equals("")).ToArray();

            path[1] = path[1].ToString().Replace('-', '_').ToLower();
            return path;
        }

        private string GetSerializedQueryParameters(HttpRequest request)
        {
            var query = new Dictionary<string, string>();

            foreach (var item in request.Query) {
                query.Add(item.Key, item.Value);
            }

            return JsonConvert.SerializeObject(query);
        }

        private string GetSerializedBody(HttpRequest request)
        {
            HttpRequestRewindExtensions.EnableBuffering(request);

            string body = "{}";

            using (StreamReader reader = new StreamReader(request.Body, leaveOpen: true))
            {
                string result = reader.ReadToEndAsync().Result;

                if (result.Length > 0) {
                    var deserializedBody = JsonConvert.DeserializeObject(result);
                    body = JsonConvert.SerializeObject(deserializedBody);
                }

                request.Body.Position = 0;
            }

            return body;
        }
    }
}
