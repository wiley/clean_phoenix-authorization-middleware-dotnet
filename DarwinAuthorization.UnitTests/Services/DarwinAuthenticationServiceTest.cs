using Castle.Core.Logging;
using DarwinAuthorization.Models;
using DarwinAuthorization.Services;
using Keycloak.AuthServices.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarwinAuthorization.UnitTests.Services
{
    public class DarwinAuthenticationServiceTest
    {
        private DarwinAuthenticationService _authenticationService;
        private Mock<DarwinAuthorizationContext> _authorizationContextMock;
        private Mock<ILogger<DarwinAuthenticationService>> _loggerMock;

        public DarwinAuthenticationServiceTest()
        {
            _authorizationContextMock = new Mock<DarwinAuthorizationContext>();
            _loggerMock = new Mock<ILogger<DarwinAuthenticationService>>();
            _authenticationService = new DarwinAuthenticationService(_loggerMock.Object);
        }
    }
}
