using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net.Mime;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using ProductCatalog.Data;
using ProductCatalog.Models.Dtos;
using ProductCatalog.Models.Exceptions;
using ProductCatalog.Services;

namespace ProductCatalog;

public class Program
{
	public static async Task Main(string[] args)
	{

		var builder = WebApplication.CreateBuilder(args);
		IConfiguration configuration = builder.Configuration;

		if (!builder.Environment.IsDevelopment())
		{
			// builder.Configuration.AddEnvironmentVariables();
			// builder.Configuration.AddAzureKeyVault(
			// 	new Uri("https://<keyvault>.vault.azure.net/"),
			// 	new DefaultAzureCredential());
		}
		builder.Services.AddHealthChecks().AddDbContextCheck<ProductCatalogDbContext>("dbcontext", HealthStatus.Unhealthy);
		builder.Services.ConfigureDatabase(configuration);
		// builder.Services.AddDbContext<ProductCatalogDbContext>(opt =>
		// {
		// 	var connectionString = configuration.GetConnectionString("ProductCatalogDbPgSqlConnection");
		// 	opt.UseNpgsql(connectionString, sqlOptions =>
		// 		{
		// 			sqlOptions.EnableRetryOnFailure(
		// 				4,
		// 				TimeSpan.FromSeconds(Math.Pow(2, 3)),
		// 				null);
		// 		})
		// 		.UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture);
		// });

		builder.Services.AddHttpLogging(o => { });

		builder.Services.Configure<GzipCompressionProviderOptions>(options => options.Level = CompressionLevel.Optimal);
		builder.Services.AddResponseCompression(options =>
		{
			options.EnableForHttps = true;
			options.Providers.Add<GzipCompressionProvider>();
		});

		// Add services to the container.
		builder.Services.AddControllers();

		if (true || builder.Environment.IsDevelopment())
		{
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(options =>
			{
				var contact = new OpenApiContact
				{
					Name = configuration["SwaggerApiInfo:Name"]
				};

				options.SwaggerDoc("v1", new OpenApiInfo
				{
					Title = $"{configuration["SwaggerApiInfo:Title"]}",
					Version = "v1",
					Contact = contact
				});

				// options.EnableAnnotations();
				// options.SchemaFilter<CustomSchemaFilters>();
				// options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				// {
				// 	Description = @"JWT Authorization header using the Bearer scheme. \r\n\r\n 
				//                  Enter 'Bearer' [space] and then your token in the text input below.
				//                  \r\n\r\nExample: 'Bearer 12345abcdef'",
				// 	Name = "Authorization",
				// 	In = ParameterLocation.Header,
				// 	Type = SecuritySchemeType.ApiKey,
				// 	Scheme = "Bearer"
				// });
				// options.AddSecurityRequirement(new OpenApiSecurityRequirement()
				// {
				// 	{
				// 		new OpenApiSecurityScheme
				// 		{
				// 			Reference = new OpenApiReference
				// 			{
				// 				Type = ReferenceType.SecurityScheme,
				// 				Id = "Bearer"
				// 			},
				// 			Scheme = "oauth2",
				// 			Name = "Bearer",
				// 			In = ParameterLocation.Header,
				//
				// 		},
				// 		new List<string>()
				// 	}
				// });

				var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
				var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
				options.IncludeXmlComments(xmlPath);
			});
		}

		builder.Services.AddEndpointsApiExplorer();

		builder.Services.AddScoped<IProductService, ProductService>();

		var app = builder.Build();

		app.Logger.LogInformation("Seeding Database...");
		using (var scope = app.Services.CreateScope())
		{
			var scopedProvider = scope.ServiceProvider;
			try
			{
				var catalogContext = scopedProvider.GetRequiredService<ProductCatalogDbContext>();
				await ProductCatalogContextSeed.SeedAsync(catalogContext, app.Logger);
			}
			catch (Exception ex)
			{
				app.Logger.LogError(ex, "An error occurred seeding the DB.");
			}
		}

		// Configure the HTTP request pipeline.
		app.UseResponseCompression();
		app.UseAuthorization();
		app.MapControllers();
		app.UseHttpLogging();

		if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

		app.UseHealthChecks("/health/ping", new HealthCheckOptions { AllowCachingResponses = false });
		app.UseHealthChecks("/health/dbcontext", new HealthCheckOptions { AllowCachingResponses = false });

		app.UseSwagger();
		app.UseSwaggerUI(options =>
		{
			options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
			// options.RoutePrefix = string.Empty;
			// options.DisplayRequestDuration();
		});

		app.UseExceptionHandler(appBuilder =>
		{
			appBuilder.Run(async context =>
			{
				var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
				var exception = exceptionHandlerPathFeature?.Error;

				context.Response.StatusCode = exception switch
				{
					EntityNotFoundException => StatusCodes.Status404NotFound,
					_ => StatusCodes.Status500InternalServerError
				};

				var apiResponse = exception switch
				{
					EntityNotFoundException => new ApiResponse("Product not found"),
					_ => new ApiResponse("An error occurred")
				};

				context.Response.ContentType = MediaTypeNames.Application.Json;
				await context.Response.WriteAsync(JsonSerializer.Serialize(apiResponse));
			});
		});

		await app.RunAsync();
	}
}