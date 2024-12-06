using DarwinAuthorization.Models;
using DarwinAuthorization.Models.OPA;
using DarwinAuthorization.Services;
using DarwinAuthorization.UnitTests.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NSubstitute;
using System.Net.Http.Json;

namespace DarwinAuthorization.UnitTests.Services
{
    public class OPAServiceTests
    {
        private readonly ILogger<OPAService> _logger;
        private HttpRequestMessage lastRequestMessage;

        public OPAServiceTests()
        {
            _logger = Substitute.For<MockLogger<OPAService>>();
        }

        [Test]
        [TestCase("JHDF76GLN-KSAFUD68-JKASDFEWU4")]
        public void SendRequestAPIKey_OPA_Return_Success(string apiKey)
        {
            var opaService = new OPAService(_logger, GetMockedHttpClient(GetMockedOpaMessageHandler(GetMockedResult(true)).Object));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Api-Key"] = apiKey;
            httpContext.Request.Headers["X-User-Id"] = "45123";
            httpContext.Request.Headers["X-Authz-Data"] = "0";
            httpContext.Request.Path = "/v4/api/enrollments";
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "parameter1", "4815" },
                { "parameter2", "162342" }
            });

            string body = "{\"title\":\"test\"}";
            httpContext.Request.Body = GenerateStreamFromString(body);

            bool? isAuthorized = opaService.RedirectOPA(httpContext.Request, GetDarwinAuthorizationConfig()).Result;

            Assert.IsTrue(isAuthorized);

            using StreamReader reader = new(lastRequestMessage.Content.ReadAsStream());
            OPARequestContent content = JsonConvert.DeserializeObject<OPARequestContent>(
                reader.ReadToEndAsync().Result
            );

            Assert.That(
                content.Input.Query,
                Is.EqualTo("{\"parameter1\":\"4815\",\"parameter2\":\"162342\"}")
            );

            Assert.That(content.Input.Body,Is.EqualTo(body));
        }

        [Test]
        [TestCase("JHDF76GLN-KSAFUD68-JKASDFEWU4")]
        public void SendRequestAPIKey_OPA_Return_False(string apiKey)
        {
            var opaService = new OPAService(_logger, GetMockedHttpClient(GetMockedOpaMessageHandler(GetMockedResult(false)).Object));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Api-Key"] = apiKey;
            httpContext.Request.Headers["X-User-Id"] = "45123";
            httpContext.Request.Headers["X-Authz-Data"] = "1";
            httpContext.Request.Path = "/v4/api/enrollments";

            bool? isAuthorized = opaService.RedirectOPA(httpContext.Request, GetDarwinAuthorizationConfig()).Result;
            Assert.IsFalse(isAuthorized);
        }


        [Test]
        [TestCase("eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICIybjNlWkRtVTQyV1ZGZHFpeUJQUzhGX01kMzBkYUVKMmE3QVBvU0dNSm9BIn0.eyJleHAiOjE2ODI1MzE4MDMsImlhdCI6MTY4MjUzMDYwMywianRpIjoiM2NhYzVlNWItNzI2OC00NzNlLThjY2UtODc4ZDk0ZTk3NjE2IiwiaXNzIjoiaHR0cHM6Ly9rZXljbG9hay5ub25wcm9kLmRhcndpbi53aWxleS5ob3N0L3JlYWxtcy9kYXJ3aW4iLCJhdWQiOiJhY2NvdW50Iiwic3ViIjoiZWRlZTUwMDYtYjU5ZS00YTIxLThmNmQtZmMwOGM4ZDQ0ZTc4IiwidHlwIjoiQmVhcmVyIiwiYXpwIjoiZnJvbnRlbmQtY2xpZW50Iiwic2Vzc2lvbl9zdGF0ZSI6ImY5ZDg3YzFhLTE3ZGItNDAyZS05ODM0LWNlNWJjYTcxZDUyMCIsImFjciI6IjEiLCJyZWFsbV9hY2Nlc3MiOnsicm9sZXMiOlsiZGVmYXVsdC1yb2xlcy1kYXJ3aW4iLCJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIl19LCJzY29wZSI6InVzZXJfaWQiLCJzaWQiOiJmOWQ4N2MxYS0xN2RiLTQwMmUtOTgzNC1jZTViY2E3MWQ1MjAiLCJ1c2VyX2lkIjo0NTIyOTgyNX0.DEotFoHoQUY7xolEIVK89tQ8J0GTe8XRMOf9xP2jagh6rQeNnGI6nFxevZNSB1ZWN7lEmgKArWPSdRWjJFhmEqwHdxD0pL-N-xq67-fgbI6byblZHH-OM4gP0WikxhT6uM7ofEb3ncDfPToGZoh9ADRFdkCDGChuntOjgYQ_0jsQrCb4KKMdRUhOEG8t9IFwm-_UouZ2mXcZBKMLoluAqJFEGFcur1AmgBrGdcFKIiqzyANMR9tvOTx-nFBAuv9RBwUn3nV70WshYxskJyDdNsH_QRxFS4Hwgfh6ShgS-OAisCyNfW1i7GH9D_rq21hAqbLxwCw58LmJzsRQrtWfTA")]
        public void SendRequestJWT_OPA_Return_Success(string jwt)
        {
            var opaService = new OPAService(_logger, GetMockedHttpClient(GetMockedOpaMessageHandler(GetMockedResult(true)).Object));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = jwt;
            httpContext.Request.Path = "/v4/api/enrollments";

            bool? isAuthorized = opaService.RedirectOPA(httpContext.Request, GetDarwinAuthorizationConfig()).Result;
            Assert.IsTrue(isAuthorized);
        }

        [Test]
        [TestCase("eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICIybjNlWkRtVTQyV1ZGZHFpeUJQUzhGX01kMzBkYUVKMmE3QVBvU0dNSm9BIn0.eyJleHAiOjE2ODI1MzE4MDMsImlhdCI6MTY4MjUzMDYwMywianRpIjoiM2NhYzVlNWItNzI2OC00NzNlLThjY2UtODc4ZDk0ZTk3NjE2IiwiaXNzIjoiaHR0cHM6Ly9rZXljbG9hay5ub25wcm9kLmRhcndpbi53aWxleS5ob3N0L3JlYWxtcy9kYXJ3aW4iLCJhdWQiOiJhY2NvdW50Iiwic3ViIjoiZWRlZTUwMDYtYjU5ZS00YTIxLThmNmQtZmMwOGM4ZDQ0ZTc4IiwidHlwIjoiQmVhcmVyIiwiYXpwIjoiZnJvbnRlbmQtY2xpZW50Iiwic2Vzc2lvbl9zdGF0ZSI6ImY5ZDg3YzFhLTE3ZGItNDAyZS05ODM0LWNlNWJjYTcxZDUyMCIsImFjciI6IjEiLCJyZWFsbV9hY2Nlc3MiOnsicm9sZXMiOlsiZGVmYXVsdC1yb2xlcy1kYXJ3aW4iLCJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIl19LCJzY29wZSI6InVzZXJfaWQiLCJzaWQiOiJmOWQ4N2MxYS0xN2RiLTQwMmUtOTgzNC1jZTViY2E3MWQ1MjAiLCJ1c2VyX2lkIjo0NTIyOTgyNX0.DEotFoHoQUY7xolEIVK89tQ8J0GTe8XRMOf9xP2jagh6rQeNnGI6nFxevZNSB1ZWN7lEmgKArWPSdRWjJFhmEqwHdxD0pL-N-xq67-fgbI6byblZHH-OM4gP0WikxhT6uM7ofEb3ncDfPToGZoh9ADRFdkCDGChuntOjgYQ_0jsQrCb4KKMdRUhOEG8t9IFwm-_UouZ2mXcZBKMLoluAqJFEGFcur1AmgBrGdcFKIiqzyANMR9tvOTx-nFBAuv9RBwUn3nV70WshYxskJyDdNsH_QRxFS4Hwgfh6ShgS-OAisCyNfW1i7GH9D_rq21hAqbLxwCw58LmJzsRQrtWfTA")]
        public void SendRequestJWT_OPA_Return_False(string jwt)
        {

            var opaService = new OPAService(_logger, GetMockedHttpClient(GetMockedOpaMessageHandler(GetMockedResult(false)).Object));
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = jwt;
            httpContext.Request.Path = "/v4/api/enrollments";

            bool? isAuthorized = opaService.RedirectOPA(httpContext.Request, GetDarwinAuthorizationConfig()).Result;
            Assert.IsFalse(isAuthorized);
        }

        [Test]
        [TestCase("JHDF76GLN-KSAFUD68-JKASDFEWU4")]
        public void SendRequestAPIKey_OPA_Return_Null(string apiKey)
        {
            var opaService = new OPAService(_logger, GetMockedHttpClient(GetMockedNullOpaMessageHandler().Object));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Api-Key"] = apiKey;
            httpContext.Request.Headers["X-User-Id"] = "45123";
            httpContext.Request.Headers["X-Authz-Data"] = "0";
            httpContext.Request.Path = "/v4/api/enrollments";

            bool? isAuthorized = opaService.RedirectOPA(httpContext.Request, GetDarwinAuthorizationConfig()).Result;
            Assert.IsFalse(isAuthorized);
        }

        [Test]
        [TestCase("JHDF76GLN-KSAFUD68-JKASDFEWU4")]
        public void SendRequestAPIKey_OPA_Result_Null(string apiKey)
        {
            var opaService = new OPAService(_logger, GetMockedHttpClient(GetMockedOpaMessageHandler(null).Object));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Api-Key"] = apiKey;
            httpContext.Request.Headers["X-User-Id"] = "45123";
            httpContext.Request.Headers["X-Authz-Data"] = "0";
            httpContext.Request.Path = "/v4/api/enrollments";

            bool? isAuthorized = opaService.RedirectOPA(httpContext.Request, GetDarwinAuthorizationConfig()).Result;
            Assert.IsFalse(isAuthorized);
        }

        [Test]
        [TestCase("eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICIybjNlWkRtVTQyV1ZGZHFpeUJQUzhGX01kMzBkYUVKMmE3QVBvU0dNSm9BIn0.eyJleHAiOjE2ODI1MzE4MDMsImlhdCI6MTY4MjUzMDYwMywianRpIjoiM2NhYzVlNWItNzI2OC00NzNlLThjY2UtODc4ZDk0ZTk3NjE2IiwiaXNzIjoiaHR0cHM6Ly9rZXljbG9hay5ub25wcm9kLmRhcndpbi53aWxleS5ob3N0L3JlYWxtcy9kYXJ3aW4iLCJhdWQiOiJhY2NvdW50Iiwic3ViIjoiZWRlZTUwMDYtYjU5ZS00YTIxLThmNmQtZmMwOGM4ZDQ0ZTc4IiwidHlwIjoiQmVhcmVyIiwiYXpwIjoiZnJvbnRlbmQtY2xpZW50Iiwic2Vzc2lvbl9zdGF0ZSI6ImY5ZDg3YzFhLTE3ZGItNDAyZS05ODM0LWNlNWJjYTcxZDUyMCIsImFjciI6IjEiLCJyZWFsbV9hY2Nlc3MiOnsicm9sZXMiOlsiZGVmYXVsdC1yb2xlcy1kYXJ3aW4iLCJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIl19LCJzY29wZSI6InVzZXJfaWQiLCJzaWQiOiJmOWQ4N2MxYS0xN2RiLTQwMmUtOTgzNC1jZTViY2E3MWQ1MjAiLCJ1c2VyX2lkIjo0NTIyOTgyNX0.DEotFoHoQUY7xolEIVK89tQ8J0GTe8XRMOf9xP2jagh6rQeNnGI6nFxevZNSB1ZWN7lEmgKArWPSdRWjJFhmEqwHdxD0pL-N-xq67-fgbI6byblZHH-OM4gP0WikxhT6uM7ofEb3ncDfPToGZoh9ADRFdkCDGChuntOjgYQ_0jsQrCb4KKMdRUhOEG8t9IFwm-_UouZ2mXcZBKMLoluAqJFEGFcur1AmgBrGdcFKIiqzyANMR9tvOTx-nFBAuv9RBwUn3nV70WshYxskJyDdNsH_QRxFS4Hwgfh6ShgS-OAisCyNfW1i7GH9D_rq21hAqbLxwCw58LmJzsRQrtWfTA")]
        public void SendRequestJWT_OPA_Return_Null(string jwt)
        {
            var opaService = new OPAService(_logger, GetMockedHttpClient(GetMockedNullOpaMessageHandler().Object));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = jwt;
            httpContext.Request.Path = "/v4/api/enrollments";

            bool? isAuthorized = opaService.RedirectOPA(httpContext.Request, GetDarwinAuthorizationConfig()).Result;
            Assert.IsFalse(isAuthorized);
        }

        [Test]
        [TestCase("eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICIybjNlWkRtVTQyV1ZGZHFpeUJQUzhGX01kMzBkYUVKMmE3QVBvU0dNSm9BIn0.eyJleHAiOjE2ODI1MzE4MDMsImlhdCI6MTY4MjUzMDYwMywianRpIjoiM2NhYzVlNWItNzI2OC00NzNlLThjY2UtODc4ZDk0ZTk3NjE2IiwiaXNzIjoiaHR0cHM6Ly9rZXljbG9hay5ub25wcm9kLmRhcndpbi53aWxleS5ob3N0L3JlYWxtcy9kYXJ3aW4iLCJhdWQiOiJhY2NvdW50Iiwic3ViIjoiZWRlZTUwMDYtYjU5ZS00YTIxLThmNmQtZmMwOGM4ZDQ0ZTc4IiwidHlwIjoiQmVhcmVyIiwiYXpwIjoiZnJvbnRlbmQtY2xpZW50Iiwic2Vzc2lvbl9zdGF0ZSI6ImY5ZDg3YzFhLTE3ZGItNDAyZS05ODM0LWNlNWJjYTcxZDUyMCIsImFjciI6IjEiLCJyZWFsbV9hY2Nlc3MiOnsicm9sZXMiOlsiZGVmYXVsdC1yb2xlcy1kYXJ3aW4iLCJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIl19LCJzY29wZSI6InVzZXJfaWQiLCJzaWQiOiJmOWQ4N2MxYS0xN2RiLTQwMmUtOTgzNC1jZTViY2E3MWQ1MjAiLCJ1c2VyX2lkIjo0NTIyOTgyNX0.DEotFoHoQUY7xolEIVK89tQ8J0GTe8XRMOf9xP2jagh6rQeNnGI6nFxevZNSB1ZWN7lEmgKArWPSdRWjJFhmEqwHdxD0pL-N-xq67-fgbI6byblZHH-OM4gP0WikxhT6uM7ofEb3ncDfPToGZoh9ADRFdkCDGChuntOjgYQ_0jsQrCb4KKMdRUhOEG8t9IFwm-_UouZ2mXcZBKMLoluAqJFEGFcur1AmgBrGdcFKIiqzyANMR9tvOTx-nFBAuv9RBwUn3nV70WshYxskJyDdNsH_QRxFS4Hwgfh6ShgS-OAisCyNfW1i7GH9D_rq21hAqbLxwCw58LmJzsRQrtWfTA")]
        public void SendRequestJWT_OPA_Result_Null(string jwt)
        {
            var opaService = new OPAService(_logger, GetMockedHttpClient(GetMockedOpaMessageHandler(null).Object));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers.Authorization = jwt;
            httpContext.Request.Path = "/v4/api/enrollments";

            bool? isAuthorized = opaService.RedirectOPA(httpContext.Request, GetDarwinAuthorizationConfig()).Result;
            Assert.IsFalse(isAuthorized);
        }

        [Test]
        public void SendRequest_OPA_Exception()
        {
            var opaService = new OPAService(_logger, GetMockedHttpClient(GetMockedOpaMessageHandlerException().Object));

            var httpContext = new DefaultHttpContext();
            httpContext.Request.Path = "/v4/api/enrollments";

            Assert.ThrowsAsync<Exception>(async () =>
            {
                await opaService.RedirectOPA(httpContext.Request, GetDarwinAuthorizationConfig());
            });
        }

        private DarwinAuthorizationConfig GetDarwinAuthorizationConfig()
        {
            return new DarwinAuthorizationConfig
            {
                KeyCloakAudience = "account",
                KeyCloakRealm = "darwin",
                KeyCloakBaseUrl = "http://keycloak-local:8080",
                ServiceApiKey = "JHDF76GLN-KSAFUD68-JKASDFEWU4",
                OpaBaseUrl = "http://opa-local:8080"
            };
        }

        private Mock<HttpMessageHandler> GetMockedOpaMessageHandlerException()
        {
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Throws(new Exception());
            return httpMessageHandlerMock;
        }

        private Mock<HttpMessageHandler> GetMockedOpaMessageHandler(Result expectedResult)
        {
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            HttpResponseMessage httpResponseMessage = new()
            {
                Content = JsonContent.Create(new OpaResult
                {
                    Result = expectedResult
                })
            };

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage)
                .Callback<HttpRequestMessage, CancellationToken>((message, token) => lastRequestMessage = message);

            return httpMessageHandlerMock;
        }

        private Mock<HttpMessageHandler> GetMockedNullOpaMessageHandler()
        {
            var httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            HttpResponseMessage httpResponseMessage = new()
            {
                Content = null
            };

            httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponseMessage);

            return httpMessageHandlerMock;
        }

        private HttpClient GetMockedHttpClient(HttpMessageHandler expectedResult)
        {
            return new HttpClient(expectedResult)
            {
                BaseAddress = new Uri(GetDarwinAuthorizationConfig().OpaBaseUrl)
            };
        }

        private Result GetMockedResult(bool expectedResult)
        {
            return new Result
            {
                allow = expectedResult
            };
        }

        private static Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new();
            StreamWriter writer = new(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }
    }
}
