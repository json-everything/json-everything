using System.Collections.Generic;
using System.Text.Json;

namespace TryJsonEverything.Models
{
	public class PointerProcessOutput
	{
		public JsonElement? Result { get; set; }
		public List<string> Errors { get; set; }
	}
}