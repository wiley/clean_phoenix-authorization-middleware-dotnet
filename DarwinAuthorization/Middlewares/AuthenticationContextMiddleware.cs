using DarwinAuthorization.Models;
using DarwinAuthorization.Utils;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace DarwinAuthorization.Middlewares
{
    public class AuthenticationContextMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationContextMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, DarwinAuthorizationContext darwinAuthorizationContext)
        {
            bool hasApiKey = false;

            // Extract UserId from JWT if present
            var userId = GetUserIdFromJwt(context);

            // Extract HasJwt flag
            bool hasJwt = !string.IsNullOrEmpty(userId);

            // Extract UserId from X-User-Id header if present
            if (!hasJwt && context.Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader))
            {
                context.Request.Headers.TryGetValue("X-User-Id", out var userIdHeader);
                userId = userIdHeader;
                // Set the API Key Flag
                hasApiKey = true;
            }

            // Add the variables to the HttpContext
            darwinAuthorizationContext.HasJwt = hasJwt;
            darwinAuthorizationContext.HasApiKey = hasApiKey;
            if(!string.IsNullOrEmpty(userId))
            {
                if(int.TryParse(userId, out var userIdInt))
                    darwinAuthorizationContext.UserId = userIdInt;
            }
                
            
            // Call the next middleware in the pipeline
            await _next(context);
        }

        private string GetUserIdFromJwt(HttpContext context)
        {
            try
            {
                // Extract UserId from JWT if present
                var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (!string.IsNullOrEmpty(token))
                {
                    var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
                    var userId = jwt.Claims.FirstOrDefault(c => c.Type == "user_id")?.Value;
                    return userId;
                }

                return null; // If no JWT is present
            }
            catch
            {
                /* If extraction fails return null, it will be handled as unauthorized */
                return null;
            }
        }
    }
}
