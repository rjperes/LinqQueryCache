using System.ComponentModel.DataAnnotations.Schema;

namespace LinqQueryCache.Test.Model
{
    [Table(nameof(Blog))]
    public class Blog
    {
        public int BlogId { get; set; }
        public required string Name { get; set; }
        public required string Url { get; set; }
        public DateTime Creation { get; set; }
    }
}