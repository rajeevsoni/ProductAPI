using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Products.Data.DBContext
{
    public class ProductDBContextFactory : IDesignTimeDbContextFactory<ProductDBContext>
    {
        public ProductDBContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory()))
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = config.GetConnectionString("ProductInfoDatabase");
            var builder = new DbContextOptionsBuilder<ProductDBContext>();
            builder.UseSqlServer(connectionString,
                optionsBuilder => optionsBuilder.MigrationsAssembly(typeof(ProductDBContextFactory).GetTypeInfo().Assembly.GetName().Name));

            return new ProductDBContext(builder.Options);
        }
    }
}
