using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace LinqQueryCache
{
    public static class QueryCache
    {
        public static IMemoryCache Default { get; set; } = new MemoryCache(Options.Create<MemoryCacheOptions>(new MemoryCacheOptions()));
    }
}
