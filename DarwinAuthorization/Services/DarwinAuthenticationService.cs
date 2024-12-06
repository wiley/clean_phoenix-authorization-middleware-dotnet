using DarwinAuthorization.AuthzHandlers;
using DarwinAuthorization.Models;
using Keycloak.AuthServices.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;

namespace DarwinAuthorization.Services
{
    public class DarwinAuthenticationService
    {
        private readonly ILogger<DarwinAuthenticationService> _logger;

        public DarwinAuthenticationService(ILogger<DarwinAuthenticationService> logger)
        {
            _logger = logger;
        }

        public void ConfigureAuthentication(IServiceCollection services, DarwinAuthorizationConfig config)
        {

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            var keycloakOptions = new KeycloakAuthenticationOptions()
            {
                AuthServerUrl = config.KeyCloakBaseUrl,
                Realm = config.KeyCloakRealm,
                Resource = config.KeyCloakAudience,
                SslRequired = "external"
            };
            services.AddKeycloakAuthentication(keycloakOptions, jwtOptions =>
            {
                if (Debugger.IsAttached || config.DevEnv)
                {
                    jwtOptions.RequireHttpsMetadata = false; // Bypass token in debug mode
                }
                else
                {
                    jwtOptions.RequireHttpsMetadata = true;
                }
            });

            services.AddAuthentication("ApiKey").AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes("ApiKey", JwtBearerDefaults.AuthenticationScheme)
                    .AddRequirements(new PolicyRequirement())
                    .Build();
            });
        }
    }
}
