using System.Text.Json;
using Json.Logic;
using TryJsonEverything.Services;

namespace TryJsonEverything.Models
{
	[Schema(typeof(InputValidationSchemas), nameof(InputValidationSchemas.LogicInputSchema))]
	public class LogicProcessInput
	{
		public JsonDocument Data { get; set; }
		public Rule Logic { get; set; }
	}
}