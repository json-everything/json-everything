using System.Text.Json;
using Json.Logic;

namespace TryJsonEverything.Models
{
	public class LogicProcessInput
	{
		public JsonDocument Data { get; set; }
		public Rule Logic { get; set; }
	}
}