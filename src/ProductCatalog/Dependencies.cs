using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Data;

namespace ProductCatalog
{
	public static class Dependencies
	{
		public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
		{
			bool useOnlyInMemoryDatabase = false;
			if (configuration["UseOnlyInMemoryDatabase"] != null)
			{
				useOnlyInMemoryDatabase = bool.Parse(configuration["UseOnlyInMemoryDatabase"]!);
			}

			if (useOnlyInMemoryDatabase)
			{
				services.AddDbContext<ProductCatalogDbContext>(c =>
					c.UseInMemoryDatabase("MarketDb"));
			}
			else
			{
				services.AddDbContext<ProductCatalogDbContext>(c =>
				{
					var connectionString = configuration.GetConnectionString("ProductCatalogDbPgSqlConnection");
					c.UseNpgsql(connectionString, sqlOptions =>
						{
							sqlOptions.EnableRetryOnFailure(
								4,
								TimeSpan.FromSeconds(Math.Pow(2, 3)),
								null);
						})
						.UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture);
				});
			}
		}
	}
}
