using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Npgsql.NameTranslation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace API.DataAccess {
    public class APIDbContext : DbContext {
        public DbSet<UserData> Users { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            base.OnConfiguring(optionsBuilder);
            // FOR DEBUGGING
            //optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        }
        public APIDbContext(DbContextOptions<APIDbContext> options) : base(options) { }
        public IQueryable<UserData> UserQuery() {
            IQueryable<UserData> query = Users;
            query = query.OrderBy(p => p.Id).AsSplitQuery();
            return query;
        }
        public Task<UserData> GetUser(string name, bool allowNull = false)
            => GetUser(u => u.LowerName == name.ToLower(), allowNull);
        public async Task<UserData> GetUser(Expression<Func<UserData, bool>> pred, bool allowNull = false) {
            ChangeTracker.LazyLoadingEnabled = false;
            var query = UserQuery();
            var player = allowNull ? await query.FirstOrDefaultAsync(pred) : await query.FirstOr404(pred);
            return player;
        }

        protected override void OnModelCreating(ModelBuilder model) {
            base.OnModelCreating(model);
            UserData.Configure(model);
        }
    }
}
