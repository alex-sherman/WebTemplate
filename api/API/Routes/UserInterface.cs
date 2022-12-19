using API.DataAccess;
using API.DataAccess.PasswordSecurity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Replicate;
using Replicate.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;

namespace API.Routes {

    [ReplicateType]
    [ReplicateRoute(Route = "api/users")]
    public class UserInterface : ReplicateWebRPC {
        public UserInterface(IServiceProvider services) : base(services) { }

        [ReplicateIgnore]
        public static IQueryable<UserData> UserLookup(APIDbContext db, string name) {
            return db.Users.Where(
                    u => u.Email.ToLower() == name.ToLower()
                    || u.Name.ToLower() == name.ToLower());
        }
        [ReplicateType]
        public class UserAuthRequest {
            public string Email;
            public string Password;
        }
        [ReplicateRoute(Route = "Auth")]
        public Task<string> auth(UserAuthRequest request) {
            if (request?.Email == null || request?.Password == null) return null;
            return Services.DoWithDB(async db => {
                var user = await UserLookup(db, request.Email).FirstOrDefaultAsync();
                if (user == null || !PasswordStorage.VerifyPassword(request.Password, user.Hash))
                    throw new HTTPError("Invalid user/pass", 401);
                return Auth.GenerateToken(user);
            });
        }
        [ReplicateType]
        public class UserCreateRequest {
            public string Name;
            public string Email;
            public string Password;
        }
        public async Task<string> Add(UserCreateRequest create, bool sendVerification = true) {
            if (create?.Email == null || create.Password == null || create.Name == null) throw new HTTPError("Invalid request", 400);
            if (!create.Email.Contains("@")) throw new HTTPError("Invalid email address", 400);
            if (create.Name.Contains("@")) throw new HTTPError("Invalid name", 400);
            var user = await Services.DoWithDB(async db => {
                var _user = (await db.Users.AddAsync(new UserData() {
                    Id = 0,
                    Email = create.Email,
                    Name = create.Name,
                    Hash = PasswordStorage.CreateHash(create.Password),
                })).Entity;
                return _user;
            });
            return Auth.GenerateToken(user);
        }
        [ReplicateType]
        public class PasswordChangeRequest {
            public string Password;
        }
        [AuthRequired]
        public async Task ChangePassword(PasswordChangeRequest request) {
            UserData user = HttpContext.GetUser();
            await Services.DoWithDB(db => {
                db.Attach(user);
                user.Hash = PasswordStorage.CreateHash(request.Password);
            });
        }
    }
}
