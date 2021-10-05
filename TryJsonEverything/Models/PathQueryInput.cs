using System.Text.Json;
using TryJsonEverything.Services;
#pragma warning disable 8618

namespace TryJsonEverything.Models
{
	[Schema(typeof(InputValidationSchemas), nameof(InputValidationSchemas.PathInputSchema))]
	public class PathQueryInput
	{
		public JsonDocument Data { get; set; }
		public string Path { get; set; }
		public PathQueryOptionsInput? Options { get; set; }
	}
}