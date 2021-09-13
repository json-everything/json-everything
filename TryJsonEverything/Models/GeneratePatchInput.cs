using System.Text.Json;
using TryJsonEverything.Services;
#pragma warning disable 8618

namespace TryJsonEverything.Models
{
	[Schema(typeof(InputValidationSchemas), nameof(InputValidationSchemas.GeneratePatchInputSchema))]
	public class GeneratePatchInput
	{
		public JsonDocument Start { get; set; }
		public JsonDocument Target { get; set; }
	}
}