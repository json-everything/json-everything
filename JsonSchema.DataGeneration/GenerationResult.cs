using System.Text.Json;

namespace Json.Schema.DataGeneration;

public class GenerationResult
{
	public JsonElement Result { get; }
	public string? ErrorMessage { get; }

	private GenerationResult(JsonElement? result, string? errorMessage)
	{
		Result = result ?? default;
		ErrorMessage = errorMessage;
	}

	public static GenerationResult Success(JsonElement result)
	{
		return new GenerationResult(result, null);
	}

	public static GenerationResult Fail(string errorMessage)
	{
		return new GenerationResult(null, errorMessage);
	}
}