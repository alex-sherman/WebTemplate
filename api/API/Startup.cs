using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Replicate.MetaData;
using Replicate.Serialization;
using Replicate.RPC;
using Replicate;
using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.Routing;
using System.Text.RegularExpressions;
using Replicate.Web;
using System.Diagnostics;
using API.DataAccess;

namespace API {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public static void ConfigureReplicate() {
            ReplicationModel.Default.DictionaryAsObject = true;
            ReplicationModel.Default.LoadTypes(typeof(Startup).Assembly);
            ReplicationModel.Default[typeof(Guid)]
                .SetSurrogate(Surrogate.Simple<Guid, string>(g => g.ToString(), Guid.Parse));
        }
        public void ConfigureServices(IServiceCollection services) {
            ConfigureReplicate();
            var serializer = new JSONSerializer(ReplicationModel.Default);
            services.AddSingleton<IReplicateSerializer>(new JSONSerializer(ReplicationModel.Default,
                new JSONSerializer.Configuration() { Strict = false, }));
            services.AddSingleton(serializer);
            services.AddDbContext<APIDbContext>(options => {
                options.EnableSensitiveDataLogging(true);
                options.UseNpgsql(Configuration.GetConnectionString("Database"));
                options.UseSnakeCaseNamingConvention();
            });
            services.AddHttpContextAccessor();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider services) {
            using (var db = services.GetService<APIDbContext>()) {
                db.Database.Migrate();
            }

            if (env.GetEnvironmentType() == EnvironmentType.Development) {
                app.UseDeveloperExceptionPage();
            }
            app.Use(async (context, next) => {
                context.Response.Headers.Add("Access-Control-Allow-Methods", "GET, POST");
                context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                context.Response.Headers.Add("Access-Control-Allow-Headers", "*");
                context.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
                if (context.Request.Method != "OPTIONS")
                    await next();
            });
            app.UseRouting();
            app.UseErrorHandling(services.GetRequiredService<IReplicateSerializer>());
            app.UseEndpoints(env, ReplicationModel.Default);
        }
    }
}
