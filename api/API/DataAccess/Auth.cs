using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Replicate.Serialization;
using Replicate.Web;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace API.DataAccess {
    public class AuthRequiredAttribute : RPCMiddlewareAttribute {
        private void SetUserFromAuthorizationHeader(HttpContext context) {
            ClaimsPrincipal authedUser = null;
            var config = context.RequestServices.GetRequiredService<IConfiguration>();
            if (!context.Request.Headers["Authorization"].IsNullOrEmpty()) {
                var token = context.Request.Headers["Authorization"][0];
                authedUser = Auth.ValidateToken(token);
            }
            if (authedUser == null && config.GetValue("FakeAuth", false) && !context.Request.Headers["User"].IsNullOrEmpty()) {
                authedUser = new ClaimsPrincipal(new[] {
                    new ClaimsIdentity(new[] {
                        new Claim(User.NAME_CLAIM, context.Request.Headers["User"]),
                    }, "Email")
                });
            }
            if (authedUser != null) context.User = authedUser;
        }
        public override async Task Run(HttpContext context) {
            SetUserFromAuthorizationHeader(context);
            var user = User.From(context);

            UserData userData = await context.RequestServices.DoWithDB(db => db.GetUser(user, true));
            if (userData == null) throw Auth.MissingUser;

            ILogger logger = context.RequestServices.GetService<ILogger<AuthRequiredAttribute>>();
            logger?.LogDebug($"({context.TraceIdentifier}) USER: {context.JSONSerializer().SerializeString(userData)}");
            context.SetUser(userData);
        }
    }

    public static class Auth {
        public static HTTPError NotLoggedIn => new HTTPError("Not logged in", 400);
        public static HTTPError UnathorizedError => new HTTPError("Unauthorized", 401);
        public static HTTPError PrivilegeError => new HTTPError("Insufficient Privilege", 403);
        public static HTTPError MissingUser => new HTTPError("No account found.", 404);

        static readonly byte[] key = Convert.FromBase64String("GmrcXa0qC4i1imlKn2H2tioOO3/aF0PmgVUqZMv7STrBCTU+OCQgj1Lmuf+r6POFiQWdvJO8YFR4J0TFC9HQGQ==");
        public static string GenerateToken(UserData user) {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(User.NAME_CLAIM, user.Name),
                }),
                Expires = DateTime.UtcNow.AddDays(28),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var stoken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(stoken);

            return token;
        }
        public static string ValidateUserEmail(string token) => ValidateToken(token)?.FindFirstValue(ClaimTypes.Email);
        public static ClaimsPrincipal ValidateToken(string token) {
            try {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                    return null;

                var validationParameters = new TokenValidationParameters() {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                };

                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out securityToken);

                return principal;
            } catch {
                return null;
            }
        }
        public static async Task<UserData> Authenticate(HttpRequest request, IServiceProvider services) {
            string token = null;
            if (request.Headers.ContainsKey("Authorization")) {
                var bearer = request.Headers["Authorization"].FirstOrDefault(h => h.Substring(0, 6) == "Bearer");
                if (bearer != null && bearer.Length > 7)
                    token = bearer.Substring(7, bearer.Length - 7);
            }
            if (token == null)
                request.Cookies.TryGetValue("token", out token);
            if (token != null) {
                var email = ValidateUserEmail(token);
                if (email != null)
                    return await GetUser(services, email);
            }
            return null;
        }
        public static async Task<UserData> GetUser(IServiceProvider services, string email) {
            return await services.DoWithDB(async db => await db.Users
                .FirstOrDefaultAsync(u => u.Email == email));
        }
    }
}
