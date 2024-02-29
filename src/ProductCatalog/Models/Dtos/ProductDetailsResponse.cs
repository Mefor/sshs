using System;
using System.Text.Json.Serialization;

namespace ProductCatalog.Models.Dtos;

public class ProductDetailsResponse
{
	[JsonPropertyName("id")] public Guid Id { get; set; }

	[JsonPropertyName("name")] public string Name { get; set; }

	[JsonPropertyName("price")] public decimal Price { get; set; }

	[JsonPropertyName("owner")] public string Owner { get; set; }
}