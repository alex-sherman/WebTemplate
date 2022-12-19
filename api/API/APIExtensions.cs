using API.DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Replicate.Serialization;
using Replicate.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace API {
    public static class APIExtensions {
        public static Task DoWithDB(this IServiceProvider Services, Action<APIDbContext> func,
            bool autoSave = true) {
            return DoWithDB(Services, db => { func(db); return Task.CompletedTask; }, autoSave);
        }
        public static Task DoWithDB(this IServiceProvider Services, Func<APIDbContext, Task> func,
            bool autoSave = true) {
            return DoWithDB(Services, async db => { await func(db); return true; }, autoSave);
        }
        public static async Task<T> DoWithDB<T>(
            this IServiceProvider Services, Func<APIDbContext, Task<T>> func,
            bool autoSave = true) {
            using var db = Services.CreateScope().ServiceProvider.GetService<APIDbContext>();
            var result = await func(db);
            if (autoSave)
                await db.SaveChangesAsync();
            return result;
        }
        /// <summary>
        /// Adds (if not exists) the `toAdd`, otherwise returns the object matching `getIfError`. If the item was
        /// successfully added, the return value is the same as `toAdd`.
        /// </summary>
        public static async Task<T> GetOrAdd<T>(this APIDbContext db, T toAdd, Func<APIDbContext, Task<T>> getIfError) where T : class {
            await db.SaveChangesAsync();
            db.Add(toAdd);
            var result = await db.SaveChangesAsync().ContinueWith(t => {
                if (t.IsCompletedSuccessfully) return true;
                var ex = t.Exception.GetBaseException();
                if (!(ex is DbUpdateException dbce)) throw ex;
                return false;
            });
            if (!result) {
                db.Entry(toAdd).State = EntityState.Detached;
                return await getIfError(db);
            }
            return toAdd;
        }
        public static async Task<T> FirstOr404<T>(
            this IQueryable<T> query, Expression<Func<T, bool>> predicate) where T : class {
            return (await query.FirstOrDefaultAsync(predicate)) ??
                throw new HTTPError($"{typeof(T).Name} not found", 404);
        }
        public static T? FirstOrNull<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate = null) where T : struct {
            if (predicate != null) enumerable = enumerable.Where(predicate);
            return enumerable.Cast<T?>().FirstOrDefault();
        }
        public static IReplicateSerializer JSONSerializer(this HttpContext context)
            => context.RequestServices.GetRequiredService<JSONSerializer>();
        public static UserData GetUser(this HttpContext context) => (UserData)context.Items["Player"];
        public static void SetUser(this HttpContext context, UserData player) => context.Items["Player"] = player;
    }
}
