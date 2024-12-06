using DarwinAuthorization.Models;
using Microsoft.AspNetCore.Http;

namespace DarwinAuthorization.Interfaces
{
    public interface IOPAService
    {
        Task<bool> RedirectOPA(HttpRequest request, DarwinAuthorizationConfig config);
    }
}
