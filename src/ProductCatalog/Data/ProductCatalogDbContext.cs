using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Models.Entities;

namespace ProductCatalog.Data
{
	public class ProductCatalogDbContext : DbContext
	{
		public ProductCatalogDbContext(
			DbContextOptions<ProductCatalogDbContext> options)
			: base(options)
		{ }

		public DbSet<Product> Products { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			base.OnConfiguring(optionsBuilder);

			optionsBuilder
				.UseLazyLoadingProxies();
			// .UseNpgsql();
		}
	}
}
