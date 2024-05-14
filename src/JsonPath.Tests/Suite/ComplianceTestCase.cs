using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

#pragma warning disable CS8618
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Json.Path.Tests.Suite;

public class ComplianceTestCase
{
	public string Name { get; set; }
	public string Selector { get; set; }
	public JsonNode? Document { get; set; }
	public JsonArray? Result { get; set; }
	public List<JsonArray>? Results { get; set; }
	[JsonPropertyName("invalid_selector")]
	public bool InvalidSelector { get; set; }

	public override string ToString()
	{
		return $"Name:     {Name}\n" +
			   $"Selector: {Selector}\n" +
			   $"Document: {JsonSerializer.Serialize(Document, SerializerOptions.Default)}\n" +
			   $"Result:   {JsonSerializer.Serialize(Result, SerializerOptions.Default)}\n" +
			   $"Results:   {JsonSerializer.Serialize(Results, SerializerOptions.Default)}\n" +
			   $"IsValid:  {!InvalidSelector}";
	}
}