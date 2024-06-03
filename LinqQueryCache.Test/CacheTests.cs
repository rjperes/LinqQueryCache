using LinqQueryCache.Test.Model;
using LinqQueryCache;

namespace LinqQueryCache.Test
{
    public class CacheTests
    {
        static BlogContext GetContext()
        {
            return new BlogContext(@"Server=.;Integrated Security=SSPI;Database=Test;Trust Server Certificate=true");
        }

        [Fact]
        public void CanHit()
        {
            using var ctx = GetContext();

            var blogs1 = ctx.Blogs.Where(x => x.Url != null).AsCacheable(TimeSpan.FromSeconds(10)).ToList();

            var blogs2 = ctx.Blogs.Where(x => x.Url != null).AsCacheable().ToList();
        }

        [Fact]
        public void CanMiss()
        {
            using var ctx = GetContext();

            var blogs1 = ctx.Blogs.Where(x => x.Url != null).AsCacheable(TimeSpan.FromSeconds(10)).ToList();

            Thread.Sleep(15 * 1000);

            var blogs2 = ctx.Blogs.Where(x => x.Url != null).AsCacheable().ToList();
        }

        [Fact]
        public void CanDispose()
        {
            using var ctx = GetContext();

            var blogs1 = ctx.Blogs.Where(x => x.Url != null).AsCacheable(TimeSpan.FromMinutes(10));

            (blogs1 as IDisposable)!.Dispose();

            var blogs2 = ctx.Blogs.Where(x => x.Url != null).AsCacheable().ToList();
        }
    }
}
