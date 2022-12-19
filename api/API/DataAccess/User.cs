using Microsoft.AspNetCore.Http;
using Replicate;
using Replicate.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace API.DataAccess {
    public struct User {
        public const string NAME_CLAIM = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        public string Email;
        public static User From(ClaimsPrincipal principal, IReplicateSerializer serializer) {
            var claims = principal.Claims.ToDictionary(c => c.Type, c => c);
            if (!claims.ContainsKey(NAME_CLAIM)) throw Auth.NotLoggedIn;
            return new User() {
                Email = claims[NAME_CLAIM].Value
            };
        }
        public static User From(HttpContext context)
            => From(context.User, context.JSONSerializer());
    }
}
