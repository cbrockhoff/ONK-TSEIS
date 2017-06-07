using System;
using Microsoft.AspNetCore.Http;

namespace RestAPI.Helpers
{
    public static class HttpRequstMessageExtensions
    {
        public static Guid GetAuthenticatedUser(this HttpRequest request)
        {
            if (!request.Headers.TryGetValue(Constants.AuthenticatedUserHeaderKey, out var id))
                return Guid.Empty;

            if (!Guid.TryParse(id, out var authenticatedUser))
                return Guid.Empty;

            return authenticatedUser;
        }
    }
}