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

        /// <summary>
        /// Returns a query from the cache.
        /// </summary>
        /// <typeparam name="T">The query type.</typeparam>
        /// <param name="queryable">The query.</param>
        /// <returns>The cached query.</returns>
        /// <exception cref="InvalidOperationException">Throws an exception if the cache is not set.</exception>
        public static IQueryable<T> AsCacheable<T>(this IQueryable<T> queryable) where T : class
        {
            ArgumentNullException.ThrowIfNull(queryable, nameof(queryable));

            var cache = GetCache<T>(queryable);

            if (cache == null)
            {
                throw new InvalidOperationException("Cache is not set");
            }

            var cachedQuery = new QueryableWrapper<T>(cache, queryable);

            return cachedQuery;
        }

        /// <summary>
        /// Returns and possibly adds a query to the cache.
        /// </summary>
        /// <typeparam name="T">The query type.</typeparam>
        /// <param name="queryable">The query.</param>
        /// <param name="duration">The cache duration.</param>
        /// <returns>The cached query.</returns>
        public static IQueryable<T> AsCacheable<T>(this IQueryable<T> queryable, TimeSpan duration) where T : class
        {
            ArgumentNullException.ThrowIfNull(queryable, nameof(queryable));
         
            return AsCacheable(queryable, (int)duration.TotalSeconds);
        }

        /// <summary>
        /// Returns and possibly adds a query to the cache.
        /// </summary>
        /// <typeparam name="T">The query type.</typeparam>
        /// <param name="queryable">The query.</param>
        /// <param name="durationSeconds">The cache duration in seconds.</param>
        /// <returns>The cached query.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws an exception if the duration is invalid.</exception>
        /// <exception cref="InvalidOperationException">Throws an exception if the cache is not set.</exception>
        public static IQueryable<T> AsCacheable<T>(this IQueryable<T> queryable, int durationSeconds) where T : class
        {
            ArgumentNullException.ThrowIfNull(queryable, nameof(queryable));

            if (durationSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            var cache = GetCache<T>(queryable);

            if (cache == null)
            {
                throw new InvalidOperationException("Cache is not set");
            }

            var cachedQuery = new QueryableWrapper<T>(cache, queryable, durationSeconds);

            return cachedQuery;
        }

        /// <summary>
        /// Returns and possibly adds a query to the cache.
        /// </summary>
        /// <typeparam name="T">The query type.</typeparam>
        /// <param name="queryable">The query.</param>
        /// <param name="duration">The cache duration.</param>
        /// <returns>The cached query.</returns>
        public static IOrderedQueryable<T> AsCacheable<T>(this IOrderedQueryable<T> queryable, TimeSpan duration) where T : class
        {
            ArgumentNullException.ThrowIfNull(queryable, nameof(queryable));


            return (IOrderedQueryable<T>)AsCacheable(queryable as IQueryable<T>, duration);
        }

        /// <summary>
        /// Returns and possibly adds a query to the cache.
        /// </summary>
        /// <typeparam name="T">The query type.</typeparam>
        /// <param name="queryable">The query.</param>
        /// <param name="durationSeconds">The cache duration in seconds.</param>
        /// <returns>The cached query.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Throws an exception if the duration is invalid.</exception>
        public static IOrderedQueryable<T> AsCacheable<T>(this IOrderedQueryable<T> queryable, int durationSeconds) where T : class
        {
            ArgumentNullException.ThrowIfNull(queryable, nameof(queryable));

            if (durationSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            return (IOrderedQueryable<T>)AsCacheable(queryable as IQueryable<T>, durationSeconds);
        }
    }
}