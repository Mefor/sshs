using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using ProductCatalog.Models.Entities;

namespace ProductCatalog.Data.EntityConfigurations
{
	public class ProductEntityConfiguration : IEntityTypeConfiguration<Product>
	{
		public void Configure(EntityTypeBuilder<Product> builder)
		{
			builder.ToTable("product_catalog");

			builder.HasKey(dn => dn.Id);
			builder.Property(dn => dn.Id)
				.ValueGeneratedOnAdd();

			builder.Property(dn => dn.Name)
				.IsRequired();

			builder.Property(dn => dn.Price)
				.IsRequired();

			builder.Property(dn => dn.Owner)
				.IsRequired();
		}
	}
}
