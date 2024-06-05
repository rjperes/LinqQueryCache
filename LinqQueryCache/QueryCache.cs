using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace LinqQueryCache
{
    public static class QueryCache
    {
        private static volatile int _hits;
        private static volatile int _misses;

        /// <summary>
        /// The cache to use.
        /// </summary>
        public static IMemoryCache Default { get; set; } = new MemoryCache(Options.Create<MemoryCacheOptions>(new MemoryCacheOptions()));

        /// <summary>
        /// Raised when the cache is hit.
        /// </summary>
        public static int Hits { get; }
        /// <summary>
        /// Raised when the cache is missed.
        /// </summary>
        public static int Misses { get; }

        public static void Reset()
        {
            _hits = _misses = 0;
        }

        internal static void RaiseHit()
        {
            Interlocked.Increment(ref _hits);
        }

        internal static void RaiseMiss()
        {
            Interlocked.Increment(ref _misses);
        }
    }
}
