using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JsonPath.Tests.Suite
{
	public class ReferenceImplTestCase
	{
		public string Name { get; set; }
		public string Selector { get; set; }
		public JsonElement Document { get; set; }
		public List<JsonElement> Result { get; set; }
		[JsonPropertyName("invalid_selector")]
		public bool InvalidSelector { get; set; }

		public override string ToString()
		{
			return $"Name:     {Name}\n" +
			       $"Selector: {Selector}\n" +
			       $"Document: {Document}\n" +
			       $"Result:   {Result}\n" +
			       $"IsValid:  {!InvalidSelector}";
		}
	}
}