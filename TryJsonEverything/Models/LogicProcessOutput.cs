using System.Collections.Generic;
using System.Text.Json;

namespace TryJsonEverything.Models
{
	public class LogicProcessOutput
	{
		public JsonElement? Result { get; set; }
		public IEnumerable<string> Errors { get; set; }
	}
}