using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductCatalog.Models.Entities;

namespace ProductCatalog.Data
{
	public class ProductCatalogContextSeed
	{
		public static async Task SeedAsync(ProductCatalogDbContext catalogContext, ILogger logger, int retry = 0)
		{
			var retryForAvailability = retry;
			try
			{
				if (catalogContext.Database.IsNpgsql())
				{
					await catalogContext.Database.MigrateAsync();
				}

				if (!await catalogContext.Products.AnyAsync())
				{
					await catalogContext.Products.AddRangeAsync(GetPreconfiguredProducts());
					await catalogContext.SaveChangesAsync();
				}
			}
			catch (Exception ex)
			{
				if (retryForAvailability >= 10) throw;

				retryForAvailability++;

				logger.LogError(ex.Message);
				await SeedAsync(catalogContext, logger, retryForAvailability);
				throw;
			}
		}

		static IEnumerable<Product> GetPreconfiguredProducts()
		{
			return new List<Product>
			{
				new Product("Product_0", 19.5M, "ownerName")
			};
		}
	}
}
