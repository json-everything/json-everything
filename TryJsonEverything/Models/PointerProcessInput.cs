using System.Text.Json;
using Json.Pointer;
using TryJsonEverything.Services;
#pragma warning disable 8618

namespace TryJsonEverything.Models
{
	[Schema(typeof(InputValidationSchemas), nameof(InputValidationSchemas.PointerInputSchema))]
	public class PointerProcessInput
	{
		public JsonDocument Data { get; set; }
		public JsonPointer Pointer { get; set; }
	}
}