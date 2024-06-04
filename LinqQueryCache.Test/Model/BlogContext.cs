using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
                .LogTo(x => Debugger.Log(0, "", x))
                .UseSqlServer(connectionString);

            return builder.Options;
        }

        public DbSet<Blog> Blogs { get; private set; }
    }
}
