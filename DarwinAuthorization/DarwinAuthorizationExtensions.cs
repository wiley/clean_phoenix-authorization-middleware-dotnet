using DarwinAuthorization.AuthzHandlers;
using DarwinAuthorization.Interfaces;
using DarwinAuthorization.Middlewares;
using DarwinAuthorization.Models;
using DarwinAuthorization.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DarwinAuthorization
{
    public static class DarwinAuthorizationExtensions
    {
        public static IApplicationBuilder UseDarwinAuthenticationContext(
            this IApplicationBuilder builder)
        {
            builder.UseMiddleware<AuthenticationContextMiddleware>();
            builder.UseAuthentication();
            builder.UseAuthorization();
            return builder;
        }

        public static IServiceCollection AddDarwinAuthzConfiguration(
        this IServiceCollection service)
        {
            ILogger<DarwinAuthenticationService> logger = LoggerFactory.Create(
                config => config.AddConsole()
            ).CreateLogger<DarwinAuthenticationService>();

            DarwinAuthenticationService darwinAuthenticationService = new DarwinAuthenticationService(logger);

            DarwinAuthorizationConfig darwinAuthorizationConfig = GetDarwinAuthorizationConfig();

            service.AddHttpClient<IOPAService, OPAService>(options =>
            {
                options.BaseAddress = new Uri(darwinAuthorizationConfig.OpaBaseUrl);
            });

            service.AddScoped<IAuthorizationHandler, OpaAuthorizationHandler>();
            service.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationMiddlewareResultHandler>();
            service.AddSingleton(serviceProvider => darwinAuthorizationConfig);
            service.AddScoped<DarwinAuthorizationContext>();
            service.AddSingleton<DarwinAuthenticationService>();

            /* Configure schemes for authentication using KeyCloak JWT Bearer and ApiKey */
            /* Configure authorization Policy, injecting OpaAuthorizationHandler */
            darwinAuthenticationService.ConfigureAuthentication(service, darwinAuthorizationConfig);

            return service;
        }

        private static DarwinAuthorizationConfig GetDarwinAuthorizationConfig()
        {
            return new DarwinAuthorizationConfig
            {
                KeyCloakAudience = Environment.GetEnvironmentVariable("KEYCLOAK_AUDIENCE"),
                KeyCloakRealm = Environment.GetEnvironmentVariable("KEYCLOAK_REALM"),
                KeyCloakBaseUrl = Environment.GetEnvironmentVariable("KEYCLOAK_BASE_URL"),
                ServiceApiKey = Environment.GetEnvironmentVariable("API_KEY"),
                OpaBaseUrl = Environment.GetEnvironmentVariable("OPA_BASE_URL"),
                DevEnv = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development"
            };
        }

    }
}