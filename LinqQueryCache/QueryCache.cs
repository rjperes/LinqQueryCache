using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace LinqQueryCache
{
    public static class QueryCache
    {
        /// <summary>
        /// The cache to use.
        /// </summary>
        public static IMemoryCache Default { get; set; } = new MemoryCache(Options.Create<MemoryCacheOptions>(new MemoryCacheOptions()));

        /// <summary>
        /// Raised when the cache is hit.
        /// </summary>
        public static event Action<IQueryable>? Hit;
        /// <summary>
        /// Raised when the cache is missed.
        /// </summary>
        public static event Action<IQueryable>? Miss;

        internal static void RaiseHit(IQueryable queryable)
        {
            Hit?.Invoke(queryable);
        }

        internal static void RaiseMiss(IQueryable queryable)
        {
            Miss?.Invoke(queryable);
        }
    }
}
