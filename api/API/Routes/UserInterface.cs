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
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;

namespace API.Routes {

    [ReplicateType]
    [ReplicateRoute(Route = "api/users")]
    public class UserInterface : ReplicateWebRPC {
        public UserInterface(IServiceProvider services) : base(services) { }
        [ReplicateType]
        public class UserAuthRequest {
            public string Name;
            public string Password;
        }
        [ReplicateRoute(Route = "Auth")]
        public Task<string> auth(UserAuthRequest request) {
            if (request?.Name == null || request?.Password == null) throw new HTTPError("Invalid request", 400);
            return Services.DoWithDB(async db => {
                var user = await db.GetUser(request.Name);
                if (user == null || !PasswordStorage.VerifyPassword(request.Password, user.Hash))
                    throw new HTTPError("Invalid user/pass", 401);
                return Auth.GenerateToken(user);
            });
        }
        [ReplicateType]
        public class UserCreateRequest {
            public string Name;
            public string Password;
        }
        public async Task<string> Add(UserCreateRequest create, bool sendVerification = true) {
            if (create.Password == null || create.Name == null) throw new HTTPError("Invalid request", 400);
            var user = await Services.DoWithDB(async db => {
                var _user = (await db.Users.AddAsync(new UserData() {
                    Id = 0,
                    LowerName = create.Name.ToLower(),
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
        [AuthRequired]
        public Task<UserData> Current() {
            return Task.FromResult(HttpContext.GetUser());
        }
    }
}
