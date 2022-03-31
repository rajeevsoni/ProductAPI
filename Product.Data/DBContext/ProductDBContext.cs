using Microsoft.EntityFrameworkCore;
using Products.Data.Entities;

namespace Products.Data.DBContext
{
    public class ProductDBContext : DbContext
    {
        private readonly string _connectionString;

        public ProductDBContext( DbContextOptions<ProductDBContext> options) : base(options)
        { }

        public DbSet<Product> Products { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<NextSequence> NextSequence { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            NextSequence nextSequence = new NextSequence();
            nextSequence.Id = 1;
            nextSequence.NextSequenceNumber = 10000000;
            modelBuilder.Entity<NextSequence>()
                .HasData(nextSequence);
        }
    }
}
