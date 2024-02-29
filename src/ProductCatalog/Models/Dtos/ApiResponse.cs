using System.Text.Json.Serialization;

namespace ProductCatalog.Models.Dtos;

public class ApiResponse
{
	public ApiResponse(string message)
	{
		Message = message;
	}

	[JsonPropertyName("message")] public string Message { get; }
}