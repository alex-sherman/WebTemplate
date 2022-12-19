using API;
using API.DataAccess;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Replicate.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APITests
{
    public class BaseTest
    {
        internal static bool UsePsql = true;
        protected IServiceProvider Services;
        // NOTE: We have to use SQLite in memory, since the regulary in-memory provider does't support transactions
        // The SQLite in memory provider tears down when the connection closes, so tests need to keep
        // a connection open.
        private SqliteConnection dbCon = new SqliteConnection("DataSource=:memory:");
        [TestInitialize]
        public void Setup()
        {
            Startup.ConfigureReplicate();
            var services = new ServiceCollection();
            dbCon.Open();
            if (!UsePsql)
                services.AddDbContext<APIDbContext>(options => {
                    options.EnableSensitiveDataLogging(true);
                    options.UseSqlite(dbCon);
                });
            else
                services.AddDbContext<APIDbContext>(options => {
                    options.UseNpgsql("Host=localhost;Username=postgres;Database=db-test;Password=verysecretpassword;");
                });
            Services = services.BuildServiceProvider();
        }
        [TestCleanup]
        public void Cleanup()
        {
            Services.DoWithDB(db => db.Database.EnsureDeletedAsync()).GetAwaiter().GetResult();
        }
    }
}
