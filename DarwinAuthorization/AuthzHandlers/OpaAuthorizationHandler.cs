using DarwinAuthorization.Interfaces;
using DarwinAuthorization.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace DarwinAuthorization.AuthzHandlers
{
    public class OpaAuthorizationHandler : AuthorizationHandler<PolicyRequirement>
    {
        private readonly IOPAService _opaService;
        private readonly DarwinAuthorizationConfig _config;
        private readonly DarwinAuthorizationContext _context;

        public OpaAuthorizationHandler(IOPAService opaService, DarwinAuthorizationConfig config, DarwinAuthorizationContext context)
        {
            _config = config;
            _opaService = opaService;
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PolicyRequirement requirement)
        {
            /* If it's not authenticated then doesn't check for Authorization against OPA */
            if (!context.User.Identity.IsAuthenticated)
                return;   

            /* Check the authorization against OPA if the authentication suceeded */
            if (context.Resource is HttpContext httpContext)
            {
                var request = httpContext.Request;
                if(_context.HasJwt)
                {
                    bool allow = _opaService.RedirectOPA(request, _config).Result;
                    if (allow)
                    {
                        context.Succeed(requirement);
                        return;
                    }
                }
                else
                {
                    context.Succeed(requirement);
                    return;
                }
                context.Fail();
            }
        }
    }

    public class PolicyRequirement : IAuthorizationRequirement { }
}
