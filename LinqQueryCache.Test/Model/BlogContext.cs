using Microsoft.EntityFrameworkCore;

namespace LinqQueryCache.Test.Model
{
    public class BlogContext : DbContext
    {
        public BlogContext(DbContextOptions options) : base(options)
        {
        }

        public BlogContext(string connectionString) : this(GetOptions(connectionString))
        {
        }

        private static DbContextOptions GetOptions(string connectionString)
        {
            var builder = new DbContextOptionsBuilder()
                .UseSqlServer(connectionString);

            return builder.Options;
        }

        public DbSet<Blog> Blogs { get; private set; }
    }
}
