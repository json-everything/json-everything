using System.Text.Json;
using Json.Pointer;

namespace TryJsonEverything.Models
{
	public class PointerProcessInput
	{
		public JsonDocument Data { get; set; }
		public JsonPointer Pointer { get; set; }
	}
}