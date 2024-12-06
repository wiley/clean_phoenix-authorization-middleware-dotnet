using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DarwinAuthorization.UnitTests
{
    public static class FakeHttpClient
    {
        public static HttpClient GetFakeAPIKeyClient(string apiHeader, string apiKey, HttpStatusCode statusCode, object expected)
        {
            var fakeHttpMessageHandler = new FakeHttpMessageHandler(apiHeader, apiKey, new HttpResponseMessage()
            {
                StatusCode = statusCode,
                Content = new StringContent(JsonConvert.SerializeObject(expected), Encoding.UTF8, "application/json")
            });
            var fakeClient = new HttpClient(fakeHttpMessageHandler);
            fakeClient.BaseAddress = new Uri("http://www.wiley-epic.com");

            return fakeClient;
        }

        public static HttpClient GetFakeClientSimple(HttpStatusCode statusCode, string expected)
        {
            var fakeHttpMessageHandler = new FakeHttpMessageHandlerSimple(new HttpResponseMessage()
            {
                StatusCode = statusCode,
                Content = new StringContent(expected, Encoding.UTF8, "application/json")
            });
            var fakeClient = new HttpClient(fakeHttpMessageHandler);
            fakeClient.BaseAddress = new Uri("http://www.wiley-epic.com");

            return fakeClient;
        }
    }
}
