using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace LinqQueryCache
{
    public static class QueryableExtensions
    {
        private static IMemoryCache GetCache<T>(this IQueryable<T> queryable) where T : class
        {
            return (queryable as IInfrastructure<IServiceProvider>)?.GetService<IMemoryCache>() ?? QueryCache.Default;
        }

        public static IQueryable<T> AsCacheable<T>(this IQueryable<T> queryable) where T : class
        {
            var cache = GetCache<T>(queryable);

            var cachedQuery = new QueryableWrapper<T>(cache, queryable);

            return cachedQuery;
        }

        public static IQueryable<T> AsCacheable<T>(this IQueryable<T> queryable, TimeSpan duration) where T : class
        {
            return AsCacheable(queryable, (int)duration.TotalSeconds);
        }

        public static IQueryable<T> AsCacheable<T>(this IQueryable<T> queryable, int durationSeconds) where T : class
        {
            var cache = GetCache<T>(queryable);

            var cachedQuery = new QueryableWrapper<T>(cache, queryable, durationSeconds);

            return cachedQuery;
        }

        public static IOrderedQueryable<T> AsCacheable<T>(this IOrderedQueryable<T> queryable, TimeSpan duration) where T : class
        {
            return (IOrderedQueryable<T>)AsCacheable(queryable as IQueryable<T>, duration);
        }

        public static IOrderedQueryable<T> AsCacheable<T>(this IOrderedQueryable<T> queryable, int durationSeconds) where T : class
        {
            return (IOrderedQueryable<T>)AsCacheable(queryable as IQueryable<T>, durationSeconds);
        }
    }
}
