using System.Collections.Generic;
using System.Text.Json;
using Json.More;

namespace Json.Schema.DataGeneration
{
	/// <summary>
	/// Holds the result of an instance generation operation.
	/// </summary>
	public class GenerationResult
	{
		internal static GenerationResult NotApplicable { get; } = new GenerationResult(null, null, null);

		/// <summary>
		/// Gets the resulting JSON data, if successful.
		/// </summary>
		public JsonElement Result { get; }
		/// <summary>
		/// Gets the error message from the generation, if unsuccessful.
		/// </summary>
		public string? ErrorMessage { get; }
		/// <summary>
		/// Gets the result objects from nested data generations.
		/// </summary>
		public IEnumerable<GenerationResult>? InnerResults { get; }

		/// <summary>
		/// Gets whether the data generation was successful.
		/// </summary>
		public bool IsSuccess => ErrorMessage == null && InnerResults == null;

		private GenerationResult(JsonElement? result, string? errorMessage, IEnumerable<GenerationResult>? inner)
		{
			Result = result ?? default;
			ErrorMessage = errorMessage;
			InnerResults = inner;
		}

		internal static GenerationResult Success(JsonElement result)
		{
			return new GenerationResult(result, null, null);
		}

		internal static GenerationResult Success(JsonElementProxy result)
		{
			return new GenerationResult(result, null, null);
		}

		internal static GenerationResult Fail(string errorMessage)	
		{
			return new GenerationResult(null, errorMessage, null);
		}

		internal static GenerationResult Fail(IEnumerable<GenerationResult> inner)
		{
			return new GenerationResult(null, null, inner);
		}
	}
}