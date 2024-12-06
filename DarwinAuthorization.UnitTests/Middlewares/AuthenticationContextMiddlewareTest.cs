using DarwinAuthorization.Middlewares;
using DarwinAuthorization.Models;
using DarwinAuthorization.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Moq.Protected;
using NSubstitute;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace DarwinAuthorization.UnitTests.Middlewares
{
    public class AuthenticationContextMiddlewareTest
    {
        [Test]
        public async Task InvokeAsync_NoJwtOrApiKey()
        {
            var middleware = new AuthenticationContextMiddleware((context) => Task.CompletedTask);
            var context = new DefaultHttpContext();
            var darwinAuthorizationContext = new DarwinAuthorizationContext();

            await middleware.InvokeAsync(context, darwinAuthorizationContext);

            Assert.False(darwinAuthorizationContext.HasJwt);
            Assert.False(darwinAuthorizationContext.HasApiKey);
            Assert.AreEqual(default(int), darwinAuthorizationContext.UserId);
        }

        [Test]
        [TestCase("eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICIybjNlWkRtVTQyV1ZGZHFpeUJQUzhGX01kMzBkYUVKMmE3QVBvU0dNSm9BIn0.eyJleHAiOjE2ODI1MzE4MDMsImlhdCI6MTY4MjUzMDYwMywianRpIjoiM2NhYzVlNWItNzI2OC00NzNlLThjY2UtODc4ZDk0ZTk3NjE2IiwiaXNzIjoiaHR0cHM6Ly9rZXljbG9hay5ub25wcm9kLmRhcndpbi53aWxleS5ob3N0L3JlYWxtcy9kYXJ3aW4iLCJhdWQiOiJhY2NvdW50Iiwic3ViIjoiZWRlZTUwMDYtYjU5ZS00YTIxLThmNmQtZmMwOGM4ZDQ0ZTc4IiwidHlwIjoiQmVhcmVyIiwiYXpwIjoiZnJvbnRlbmQtY2xpZW50Iiwic2Vzc2lvbl9zdGF0ZSI6ImY5ZDg3YzFhLTE3ZGItNDAyZS05ODM0LWNlNWJjYTcxZDUyMCIsImFjciI6IjEiLCJyZWFsbV9hY2Nlc3MiOnsicm9sZXMiOlsiZGVmYXVsdC1yb2xlcy1kYXJ3aW4iLCJvZmZsaW5lX2FjY2VzcyIsInVtYV9hdXRob3JpemF0aW9uIl19LCJzY29wZSI6InVzZXJfaWQiLCJzaWQiOiJmOWQ4N2MxYS0xN2RiLTQwMmUtOTgzNC1jZTViY2E3MWQ1MjAiLCJ1c2VyX2lkIjo0NTIyOTgyNX0.DEotFoHoQUY7xolEIVK89tQ8J0GTe8XRMOf9xP2jagh6rQeNnGI6nFxevZNSB1ZWN7lEmgKArWPSdRWjJFhmEqwHdxD0pL-N-xq67-fgbI6byblZHH-OM4gP0WikxhT6uM7ofEb3ncDfPToGZoh9ADRFdkCDGChuntOjgYQ_0jsQrCb4KKMdRUhOEG8t9IFwm-_UouZ2mXcZBKMLoluAqJFEGFcur1AmgBrGdcFKIiqzyANMR9tvOTx-nFBAuv9RBwUn3nV70WshYxskJyDdNsH_QRxFS4Hwgfh6ShgS-OAisCyNfW1i7GH9D_rq21hAqbLxwCw58LmJzsRQrtWfTA")]
        public async Task InvokeAsync_HasJwt_SetsJwtFlagAndUserId(string jwt)
        {
            // Arrange
            var middleware = new AuthenticationContextMiddleware((context) => Task.CompletedTask);
            var context = new DefaultHttpContext();
            var darwinAuthorizationContext = new DarwinAuthorizationContext();
            var userId = "45229825";
            context.Request.Headers["Authorization"] = $"Bearer {jwt}";

            await middleware.InvokeAsync(context, darwinAuthorizationContext);

            // Assert
            Assert.True(darwinAuthorizationContext.HasJwt);
            Assert.False(darwinAuthorizationContext.HasApiKey);
            Assert.AreEqual(int.Parse(userId), darwinAuthorizationContext.UserId);
        }

        [Test]
        [TestCase("fakejwt")]
        public async Task InvokeAsync_HasInvalidJwt_SetsJwtFlagFalsed(string jwt)
        {
            // Arrange
            var middleware = new AuthenticationContextMiddleware((context) => Task.CompletedTask);
            var context = new DefaultHttpContext();
            var darwinAuthorizationContext = new DarwinAuthorizationContext();
            var userId = "45229825";
            context.Request.Headers["Authorization"] = $"Bearer {jwt}";

            await middleware.InvokeAsync(context, darwinAuthorizationContext);

            // Assert
            Assert.False(darwinAuthorizationContext.HasJwt);
            Assert.False(darwinAuthorizationContext.HasApiKey);
        }
    }
}
