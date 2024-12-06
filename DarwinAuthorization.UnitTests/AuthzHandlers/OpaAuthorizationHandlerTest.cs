using DarwinAuthorization.AuthzHandlers;
using DarwinAuthorization.Interfaces;
using DarwinAuthorization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace DarwinAuthorization.UnitTests.AuthzHandlers
{
    public class OpaAuthorizationHandlerTest
    {
        private readonly Mock<IOPAService> _service;
        private readonly Mock<DarwinAuthorizationConfig> _config;
        private readonly Mock<PolicyRequirement> _policyRequirement;
        private OpaAuthorizationHandler _handler;
        private AuthorizationHandlerContext _authorizationHandlerContext;
        private readonly Mock<DarwinAuthorizationContext> _context;

        public OpaAuthorizationHandlerTest()
        {
            _service = new Mock<IOPAService>();
            _config = new Mock<DarwinAuthorizationConfig>();
            _policyRequirement = new Mock<PolicyRequirement>();
            _context = new Mock<DarwinAuthorizationContext>();
            _handler = new OpaAuthorizationHandler(_service.Object, _config.Object, _context.Object);
        }

        [Test]
        public async Task HandleRequirementAsync_UserNotAuthenticated()
        {
            var httpContext = new DefaultHttpContext();

            var identity = new Mock<ClaimsIdentity>();
            identity.Setup(identity => identity.IsAuthenticated).Returns(false);
            var user = new ClaimsPrincipal(identity.Object);

            _authorizationHandlerContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { _policyRequirement.Object }, user, httpContext);

            await _handler.HandleAsync(_authorizationHandlerContext);
            Assert.IsFalse(_authorizationHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task HandleRequirementAsync_AllowOpa()
        {
            var httpContext = new DefaultHttpContext();

            var identity = new Mock<ClaimsIdentity>();
            identity.Setup(identity => identity.IsAuthenticated).Returns(true);
            var user = new ClaimsPrincipal(identity.Object);

            _service.Setup(service => service.RedirectOPA(It.IsAny<HttpRequest>(), It.IsAny<DarwinAuthorizationConfig>()).Result).Returns(true);
            _handler = new OpaAuthorizationHandler(_service.Object, _config.Object, _context.Object);

            _authorizationHandlerContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { _policyRequirement.Object }, user, httpContext);

            await _handler.HandleAsync(_authorizationHandlerContext);
            Assert.IsTrue(_authorizationHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task HandleRequirementAsync_FailOpa()
        {
            var httpContext = new DefaultHttpContext();

            var identity = new Mock<ClaimsIdentity>();
            identity.Setup(identity => identity.IsAuthenticated).Returns(true);
            var user = new ClaimsPrincipal(identity.Object);

            _service.Setup(service => service.RedirectOPA(It.IsAny<HttpRequest>(), It.IsAny<DarwinAuthorizationConfig>()).Result).Returns(false);
            Mock<DarwinAuthorizationContext> mockContext = new Mock<DarwinAuthorizationContext>();
            mockContext.Object.HasJwt = true;
            _handler = new OpaAuthorizationHandler(_service.Object, _config.Object, mockContext.Object);

            _authorizationHandlerContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { _policyRequirement.Object }, user, httpContext);

            await _handler.HandleAsync(_authorizationHandlerContext);
            Assert.IsFalse(_authorizationHandlerContext.HasSucceeded);
        }

        [Test]
        public async Task HandleRequirementAsync_SkipOpa()
        {
            var httpContext = new DefaultHttpContext();
            var identity = new Mock<ClaimsIdentity>();
            identity.Setup(identity => identity.IsAuthenticated).Returns(true);
            var user = new ClaimsPrincipal(identity.Object);
            Mock<DarwinAuthorizationContext> mockContext = new Mock<DarwinAuthorizationContext>();
            mockContext.Object.HasJwt = false;
            _handler = new OpaAuthorizationHandler(_service.Object, _config.Object, mockContext.Object);

            _authorizationHandlerContext = new AuthorizationHandlerContext(new List<IAuthorizationRequirement> { _policyRequirement.Object }, user, httpContext);

            await _handler.HandleAsync(_authorizationHandlerContext);
            Assert.IsTrue(_authorizationHandlerContext.HasSucceeded);
        }
    }
}
