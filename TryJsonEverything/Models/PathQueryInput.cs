using System.Text.Json;

namespace TryJsonEverything.Models
{
	public class PathQueryInput
	{
		public JsonDocument Data { get; set; }
		public string Path { get; set; }
	}
}