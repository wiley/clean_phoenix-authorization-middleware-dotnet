using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization.Policy;
using AuthorizationMiddlewareResultHandler = DarwinAuthorization.AuthzHandlers.AuthorizationMiddlewareResultHandler;

namespace DarwinAuthorization.UnitTests.AuthzHandlers
{
    public class AuthorizationMiddlewareResultHandlerTest
    {
        private readonly ServiceProvider _serviceProviderMock;

        public AuthorizationMiddlewareResultHandlerTest()
        {
            var authenticationServiceMock = new Mock<IAuthenticationService>();
            _serviceProviderMock = new ServiceCollection()
                .AddSingleton(authenticationServiceMock.Object)
                .BuildServiceProvider();
        }

        [Test]
        public async Task HandleAsync_NotAuthenticated_Returns401Unauthorized()
        {
            var handler = new AuthorizationMiddlewareResultHandler();

            var identity = new ClaimsIdentity();

            var context = new DefaultHttpContext();
            context.RequestServices = _serviceProviderMock;
            context.User = new ClaimsPrincipal(identity);

            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            var authorizeResult = PolicyAuthorizationResult.Challenge();

            await handler.HandleAsync(null, context, policy, authorizeResult);

            Assert.AreEqual(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        }

        [Test]
        public async Task HandleAsync_NotAuthorized_Returns403Forbidden()
        {
            var handler = new AuthorizationMiddlewareResultHandler();

            var identityMock = new Mock<ClaimsIdentity>();
            identityMock.SetupGet(i => i.IsAuthenticated).Returns(true);

            var context = new DefaultHttpContext();
            context.RequestServices = _serviceProviderMock;
            context.User = new ClaimsPrincipal(identityMock.Object);

            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            var authorizeResult = PolicyAuthorizationResult.Forbid();

            await handler.HandleAsync(null, context, policy, authorizeResult);

            Assert.AreEqual(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        }

        [Test]
        public async Task HandleAsync_Authorized_Returns200Ok()
        {
            var handler = new AuthorizationMiddlewareResultHandler();

            var identityMock = new Mock<ClaimsIdentity>();
            identityMock.SetupGet(i => i.IsAuthenticated).Returns(true);

            var context = new DefaultHttpContext();
            context.RequestServices = _serviceProviderMock;
            context.User = new ClaimsPrincipal(identityMock.Object);

            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            var authorizeResult = PolicyAuthorizationResult.Challenge();

            await handler.HandleAsync(null, context, policy, authorizeResult);

            Assert.AreEqual(StatusCodes.Status200OK, context.Response.StatusCode);
        }
    }
}
