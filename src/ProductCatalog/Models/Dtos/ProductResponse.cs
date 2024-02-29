using System;
using System.Text.Json.Serialization;

namespace ProductCatalog.Models.Dtos
{
	public class ProductResponse
	{
		[JsonPropertyName("id")] public Guid Id { get; set; }

		[JsonPropertyName("name")] public string Name { get; set; }
	}
}