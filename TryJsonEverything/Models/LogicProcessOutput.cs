using System.Collections.Generic;
using System.Text.Json;

namespace TryJsonEverything.Models
{
	public class LogicProcessOutput
	{
		public JsonElement? Result { get; init; }
		public IEnumerable<string>? Errors { get; init; }
	}
}