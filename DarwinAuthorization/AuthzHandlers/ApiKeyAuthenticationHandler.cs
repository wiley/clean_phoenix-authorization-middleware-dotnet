using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;

namespace DarwinAuthorization.AuthzHandlers
{
    public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public ApiKeyAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeaderValues))
            {
                return AuthenticateResult.Fail("ApiKey not found in request headers.");
            }

            var apiKey = apiKeyHeaderValues.FirstOrDefault();

            if (apiKey == null)
            {
                return AuthenticateResult.Fail("Invalid ApiKey.");
            }

            var expectedApiKey = Environment.GetEnvironmentVariable("API_KEY");

            if (!string.Equals(apiKey, expectedApiKey))
            {
                return AuthenticateResult.Fail("Invalid ApiKey.");
            }

            var identity = new ClaimsIdentity(
            new List<Claim>
            {
                new Claim(ClaimTypes.Name, "ApiKeyUser")
            }, 
            Scheme.Name);

            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
