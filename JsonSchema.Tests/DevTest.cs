using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		var filePath = "C:\\Folder\\Issue435_schema.json";

		var withoutProtocol = new Uri(filePath);
		var withProtocol = new Uri($"file:///{filePath}");
		
		var fragment = new Uri("#/$defs/DerivedType", UriKind.RelativeOrAbsolute);

		var withoutProtocolResult = new Uri(withoutProtocol, fragment);
		var fileUriResult = new Uri(withProtocol, fragment);

		Console.WriteLine("File path: {0}", filePath);
		Console.WriteLine();
		Console.WriteLine("Without protocol: {0}", withoutProtocol);
		Console.WriteLine("With protocol:    {0}", withProtocol);
		Console.WriteLine();
		Console.WriteLine("Combined, Without: {0}", withoutProtocolResult);
		Console.WriteLine("Combined, With:    {0}", fileUriResult);
	}

	[Test]
	public void Test1()
	{
		var serializerOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

		var jsonDate = JsonNode.Parse("\"2023-03-20 05:41:23.3881075\"");

		Console.WriteLine(jsonDate.AsJsonString());

		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.DateTime);

		var results = schema.Evaluate(jsonDate, new EvaluationOptions
		{
			OutputFormat = OutputFormat.List,
			RequireFormatValidation = true
		});

		Console.WriteLine(JsonSerializer.Serialize(results, serializerOptions));

		var date = jsonDate.Deserialize<DateTime>();

		Console.WriteLine(date);
	}

}