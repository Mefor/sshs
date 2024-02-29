using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProductCatalog.Models.Dtos;

namespace ProductCatalog.Services
{
	public interface IProductService
	{
		Task<IEnumerable<ProductResponse>> GetAllProductsAsync();

		Task<ProductDetailsResponse> GetProductAsync(Guid id);

		Task<Guid> CreateProductAsync(CreateProductRequest request);

		Task UpdateProductAsync(Guid id, UpdateProductRequest request);

		Task DeleteProductAsync(Guid id);
	}
}