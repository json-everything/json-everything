using System.Text.Json;
using Json.Patch;

namespace TryJsonEverything.Models
{
	public class PatchProcessInput
	{
		public JsonDocument Data { get; set; }
		public JsonPatch Patch { get; set; }
	}
}