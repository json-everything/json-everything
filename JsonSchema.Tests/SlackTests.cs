using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class SlackTests
	{


	[Test]
		public void MultiDraftSelfValidation()
		{
			var json =
				@"{
					""$defs"": {
						""M"": {
							""$id"": ""http://localhost/M"",
							""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
							""$defs"": {
								""MarkConfig"": { ""type"": ""integer"" }
							}   
						},  
						""C"": {
							""$id"": ""http://localhost/C"",
							""$schema"": ""http://json-schema.org/draft-06/schema#"",
							""$defs"": {
								""Config"": { ""$ref"": ""http://localhost/M#/$defs/MarkConfig"" }
							},  
							""$ref"": ""http://localhost/C#/$defs/Config""
						}   
					},  
					""$ref"": ""/C""
				}";

			var schema = JsonSchema.FromText(json);
			var instance = JsonDocument.Parse(json).RootElement;

			var result = schema.Validate(instance, new ValidationOptions
			{
				Log = new TestLog(),
				OutputFormat = OutputFormat.Detailed,
				DefaultBaseUri = new Uri("http://localhost/")
			});

			Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions
			{
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				WriteIndented = true
			}));
		}
	}
}
