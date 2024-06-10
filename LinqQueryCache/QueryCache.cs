using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace LinqQueryCache
{
    public static class QueryCache
    {
        private static readonly object _lock = new object();

        /// <summary>
        /// The cache to use.
        /// </summary>
        public static IMemoryCache Default { get; set; } = new MemoryCache(Options.Create<MemoryCacheOptions>(new MemoryCacheOptions()));

        /// <summary>
        /// Raised when the cache is hit.
        /// </summary>
        public static int Hits { get; private set; }
        /// <summary>
        /// Raised when the cache is missed.
        /// </summary>
        public static int Misses { get; private set; }

        public static void Reset()
        {
            lock (_lock)
            {
                Hits = Misses = 0;
            }
        }

        internal static void RaiseHit()
        {
            lock (_lock)
            {
                Hits++;
            }
        }

        internal static void RaiseMiss()
        {
            lock (_lock)
            {
                Misses++;
            }
        }
    }
}
