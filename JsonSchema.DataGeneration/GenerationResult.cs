using System.Collections.Generic;
using System.Text.Json;
using Json.More;

namespace Json.Schema.DataGeneration
{
	public class GenerationResult
	{
		public JsonElement Result { get; }
		public string? ErrorMessage { get; }
		public IEnumerable<GenerationResult>? InnerResults { get; }

		public bool IsSuccess => ErrorMessage == null && InnerResults == null;

		private GenerationResult(JsonElement? result, string? errorMessage, IEnumerable<GenerationResult>? inner)
		{
			Result = result ?? default;
			ErrorMessage = errorMessage;
			InnerResults = inner;
		}

		public static GenerationResult Success(JsonElement result)
		{
			return new GenerationResult(result, null, null);
		}

		public static GenerationResult Success(JsonElementProxy result)
		{
			return new GenerationResult(result, null, null);
		}

		public static GenerationResult Fail(string errorMessage)
		{
			return new GenerationResult(null, errorMessage, null);
		}

		public static GenerationResult Fail(IEnumerable<GenerationResult> inner)
		{
			return new GenerationResult(null, null, inner);
		}
	}
}