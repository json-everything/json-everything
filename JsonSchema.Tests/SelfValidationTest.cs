using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Schema.Tests;

[TestFixture]
public class SelfValidationTest
{
	public static IEnumerable<TestCaseData> TestData =>
		new[]
		{
			new TestCaseData(MetaSchemas.Draft6) { TestName = nameof(MetaSchemas.Draft6) },

			new TestCaseData(MetaSchemas.Draft7) { TestName = nameof(MetaSchemas.Draft7) },

			new TestCaseData(MetaSchemas.Draft201909) { TestName = nameof(MetaSchemas.Draft201909) },
			new TestCaseData(MetaSchemas.Core201909) { TestName = nameof(MetaSchemas.Core201909) },
			new TestCaseData(MetaSchemas.Applicator201909) { TestName = nameof(MetaSchemas.Applicator201909) },
			new TestCaseData(MetaSchemas.Validation201909) { TestName = nameof(MetaSchemas.Validation201909) },
			new TestCaseData(MetaSchemas.Metadata201909) { TestName = nameof(MetaSchemas.Metadata201909) },
			new TestCaseData(MetaSchemas.Format201909) { TestName = nameof(MetaSchemas.Format201909) },
			new TestCaseData(MetaSchemas.Content201909) { TestName = nameof(MetaSchemas.Content201909) },

			new TestCaseData(MetaSchemas.Draft202012) { TestName = nameof(MetaSchemas.Draft202012) },
			new TestCaseData(MetaSchemas.Core202012) { TestName = nameof(MetaSchemas.Core202012) },
			new TestCaseData(MetaSchemas.Applicator202012) { TestName = nameof(MetaSchemas.Applicator202012) },
			new TestCaseData(MetaSchemas.Metadata202012) { TestName = nameof(MetaSchemas.Metadata202012) },
			new TestCaseData(MetaSchemas.FormatAnnotation202012) { TestName = nameof(MetaSchemas.FormatAnnotation202012) },
			new TestCaseData(MetaSchemas.FormatAssertion202012) { TestName = nameof(MetaSchemas.FormatAssertion202012) },
			new TestCaseData(MetaSchemas.Content202012) { TestName = nameof(MetaSchemas.Content202012) },
			new TestCaseData(MetaSchemas.Unevaluated202012) { TestName = nameof(MetaSchemas.Unevaluated202012) },
		};

	[TestCaseSource(nameof(TestData))]
	public void Hardcoded(JsonSchema schema)
	{
		var json = JsonSerializer.Serialize(schema);
		var validation = schema.Validate(JsonNode.Parse(json), new ValidationOptions { OutputFormat = OutputFormat.Hierarchical });

		validation.AssertValid();
	}

	[TestCaseSource(nameof(TestData))]
	public void Online(JsonSchema schema)
	{
		try
		{
			var localSchemaJson = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping });

			var onlineSchemaJson = new HttpClient().GetStringAsync(schema.Keywords!.OfType<IdKeyword>().Single().Id).Result;
			var onlineSchema = JsonSerializer.Deserialize<JsonSchema>(onlineSchemaJson);

			var localValidation = schema.Validate(JsonNode.Parse(onlineSchemaJson));
			var onlineValidation = onlineSchema!.Validate(JsonNode.Parse(localSchemaJson));

			try
			{
				Console.WriteLine("Asserting schema equality");
				Assert.AreEqual(onlineSchema, schema);

				Console.WriteLine("Validating local against online");
				onlineValidation.AssertValid();
				Console.WriteLine("Validating online against local");
				localValidation.AssertValid();

				Console.WriteLine("Asserting json equality");
				Assert.AreEqual(onlineSchema, schema);
			}
			catch (Exception)
			{
				Console.WriteLine("Online {0}", onlineSchemaJson);
				Console.WriteLine("Local {0}", localSchemaJson);
				throw;
			}
		}
		catch (WebException)
		{
			Assert.Inconclusive();
		}
		catch (HttpRequestException)
		{
			Assert.Inconclusive();
		}
		catch (SocketException)
		{
			Assert.Inconclusive();
		}
		catch (AggregateException e)
		{
			if (e.InnerExceptions.OfType<WebException>().Any() || e.InnerExceptions.OfType<HttpRequestException>().Any())
				Assert.Inconclusive();
			throw;
		}
	}

	[TestCaseSource(nameof(TestData))]
	public void RoundTrip(JsonSchema schema)
	{
		var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
		Console.WriteLine(json);
		var returnTrip = JsonSerializer.Deserialize<JsonSchema>(json);

		Assert.AreEqual(schema, returnTrip);
	}
}