using System.Collections.Generic;
using System.Text.Json;
#pragma warning disable 8618

namespace TryJsonEverything.Models
{
	public class PointerProcessOutput
	{
		public JsonElement? Result { get; init; }
		public IEnumerable<string> Errors { get; init; }
	}
}