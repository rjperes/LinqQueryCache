using LinqQueryCache.Test.Model;
using Microsoft.EntityFrameworkCore;

namespace LinqQueryCache.Test
{
    public class CacheTests
    {
        static BlogContext GetContext()
        {
            return new BlogContext(@"Server=.;Integrated Security=SSPI;Database=Test;Trust Server Certificate=true");
        }

        [Fact]
        public void CanGetSql()
        {
            using var ctx = GetContext();

            ctx.Blogs.Where(x => x.Creation < DateTime.Today).AsCacheable(TimeSpan.FromSeconds(10));

            var sql = ctx.Blogs.Where(x => x.Creation < DateTime.Today).AsCacheable().ToQueryString();

            Assert.NotNull(sql);
            Assert.NotEmpty(sql);
        }

        [Fact]
        public void CanHit()
        {
            using var ctx = GetContext();

            var hit = false;

            QueryCache.Hit += (q) => hit = true;

            ctx.Blogs.Where(x => x.Url != null).AsCacheable(TimeSpan.FromSeconds(10)).ToList();

            ctx.Blogs.Where(x => x.Url != null).AsCacheable().ToList();

            Assert.True(hit);
        }

        [Fact]
        public void CanMiss()
        {
            using var ctx = GetContext();

            var missed = false;

            QueryCache.Miss += (q) => missed = true;

            ctx.Blogs.Where(x => x.Url != null).AsCacheable(TimeSpan.FromSeconds(10)).ToList();

            Thread.Sleep(15 * 1000);

            ctx.Blogs.Where(x => x.Url != null).AsCacheable().ToList();

            Assert.True(missed);
        }

        [Fact]
        public void CanDispose()
        {
            using var ctx = GetContext();

            var missed = false;

            QueryCache.Miss += (q) => missed = true;

            var blogs = ctx.Blogs.Where(x => x.Url != null).AsCacheable(TimeSpan.FromMinutes(10));

            (blogs as IDisposable)!.Dispose();

            ctx.Blogs.Where(x => x.Url != null).AsCacheable().ToList();

            Assert.True(missed);
        }
    }
}
