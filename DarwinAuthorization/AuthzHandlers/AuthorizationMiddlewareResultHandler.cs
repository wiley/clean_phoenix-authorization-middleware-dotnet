using DarwinAuthorization.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;

namespace DarwinAuthorization.AuthzHandlers
{
    public class AuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
    {
        private readonly Microsoft.AspNetCore.Authorization.Policy.AuthorizationMiddlewareResultHandler defaultHandler = new();
        private readonly string STATUS_401_MESSAGE = "No valid credentials were provided.";
        private readonly string STATUS_403_MESSAGE = "The provided credentials do not have the appropriate rights for the request.";

        public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
        {
            // If the authentication failed
            if (!context.User.Identity.IsAuthenticated)
            {
                ResponseUtils.ConfigureResponse(context, StatusCodes.Status401Unauthorized, STATUS_401_MESSAGE);
                return;
            }

            // If the authorization was forbidden
            if (authorizeResult.Forbidden)
            {
                ResponseUtils.ConfigureResponse(context, StatusCodes.Status403Forbidden, STATUS_403_MESSAGE);
                return;
            }

            // Fall back to the default implementation.
            await defaultHandler.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
