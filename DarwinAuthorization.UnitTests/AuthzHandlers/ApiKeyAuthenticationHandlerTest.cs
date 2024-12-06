using DarwinAuthorization.AuthzHandlers;
using DarwinAuthorization.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace DarwinAuthorization.UnitTests.AuthzHandlers
{
    public class ApiKeyAuthenticationHandlerTest
    {

        private readonly Mock<IOptionsMonitor<AuthenticationSchemeOptions>> _options;
        private readonly Mock<ILoggerFactory> _loggerFactory;
        private readonly Mock<UrlEncoder> _encoder;
        private readonly Mock<ISystemClock> _clock;

        public ApiKeyAuthenticationHandlerTest()
        {
            _options = new Mock<IOptionsMonitor<AuthenticationSchemeOptions>>();

            // This Setup is required for .NET Core 3.1 onwards.
            _options
                .Setup(x => x.Get(It.IsAny<string>()))
                .Returns(new AuthenticationSchemeOptions());

            var logger = new Mock<ILogger<ApiKeyAuthenticationHandler>>();
            _loggerFactory = new Mock<ILoggerFactory>();
            _loggerFactory.Setup(x => x.CreateLogger(It.IsAny<String>())).Returns(logger.Object);

            _encoder = new Mock<UrlEncoder>();
            _clock = new Mock<ISystemClock>();

        }

        [Test]
        public async Task HandleAuthenticateAsync_NoApiKey_ReturnsAuthenticateResultFail()
        {
            ApiKeyAuthenticationHandler _handler = new ApiKeyAuthenticationHandler(_options.Object, _loggerFactory.Object, _encoder.Object, _clock.Object);
            var context = new DefaultHttpContext();

            await _handler.InitializeAsync(new AuthenticationScheme("ApiKey", null, typeof(ApiKeyAuthenticationHandler)), context);
            var result = await _handler.AuthenticateAsync();

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("ApiKey not found in request headers.", result.Failure.Message);
        }

        [Test]
        public async Task HandleAuthenticateAsync_ApiKeyNull_ReturnsAuthenticateResultFail()
        {
            ApiKeyAuthenticationHandler _handler = new ApiKeyAuthenticationHandler(_options.Object, _loggerFactory.Object, _encoder.Object, _clock.Object);

            var context = new DefaultHttpContext();
            var apiKey = new StringValues("");
            context.Request.Headers.Add("X-Api-Key", apiKey);

            await _handler.InitializeAsync(new AuthenticationScheme("ApiKey", null, typeof(ApiKeyAuthenticationHandler)), context);
            var result = await _handler.AuthenticateAsync();

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("Invalid ApiKey.", result.Failure.Message);
        }

        [Test]
        public async Task HandleAuthenticateAsync_InvalidApiKey_ReturnsAuthenticateResultFail()
        {
            ApiKeyAuthenticationHandler _handler = new ApiKeyAuthenticationHandler(_options.Object, _loggerFactory.Object, _encoder.Object, _clock.Object);
            Environment.SetEnvironmentVariable("API_KEY", "secret");
            var context = new DefaultHttpContext();
            var apiKey = new StringValues("fakeApiKey");
            context.Request.Headers.Add("X-Api-Key", apiKey);

            await _handler.InitializeAsync(new AuthenticationScheme("ApiKey", null, typeof(ApiKeyAuthenticationHandler)), context);
            var result = await _handler.AuthenticateAsync();

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual("Invalid ApiKey.", result.Failure.Message);
            Environment.SetEnvironmentVariable("API_KEY", null);
        }

        [Test]
        public async Task HandleAuthenticateAsync_ValidApiKey_ReturnsAuthenticateResultSuccess()
        {
            ApiKeyAuthenticationHandler _handler = new ApiKeyAuthenticationHandler(_options.Object, _loggerFactory.Object, _encoder.Object, _clock.Object);
            Environment.SetEnvironmentVariable("API_KEY", "secret");
            var context = new DefaultHttpContext();
            var apiKey = new StringValues("secret");
            context.Request.Headers.Add("X-Api-Key", apiKey);

            await _handler.InitializeAsync(new AuthenticationScheme("ApiKey", null, typeof(ApiKeyAuthenticationHandler)), context);
            var result = await _handler.AuthenticateAsync();

            Assert.IsTrue(result.Succeeded);
            Environment.SetEnvironmentVariable("API_KEY", null);
        }
    }
}
