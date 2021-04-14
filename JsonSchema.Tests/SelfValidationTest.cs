using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	[TestFixture]
	public class SelfValidationTest
	{
		public static IEnumerable<TestCaseData> TestData =>
			new[]
				{
					new TestCaseData(nameof(MetaSchemas.Draft6), MetaSchemas.Draft6),
					new TestCaseData(nameof(MetaSchemas.Draft7), MetaSchemas.Draft7),
					new TestCaseData(nameof(MetaSchemas.Draft201909), MetaSchemas.Draft201909),
					new TestCaseData(nameof(MetaSchemas.Draft202012), MetaSchemas.Draft202012),
				};

		[TestCaseSource(nameof(TestData))]
		public void Hardcoded(string testName, JsonSchema schema)
		{
			var json = JsonSerializer.Serialize(schema);
			var validation = schema.Validate(JsonDocument.Parse(json).RootElement, new ValidationOptions{OutputFormat = OutputFormat.Detailed});

			validation.AssertValid();
		}

		[TestCaseSource(nameof(TestData))]
		[Ignore("Schema equality not a feature yet")]
		public void Online(string testName, JsonSchema schema)
		{
			try
			{
				var localSchemaJson = JsonSerializer.Serialize(schema);

				var onlineSchemaJson = new HttpClient().GetStringAsync(schema.Keywords.OfType<IdKeyword>().Single().Id).Result;
				var onlineSchema = JsonSerializer.Deserialize<JsonSchema>(onlineSchemaJson);

				var localValidation = schema.Validate(JsonDocument.Parse(onlineSchemaJson).RootElement);
				var onlineValidation = onlineSchema.Validate(JsonDocument.Parse(localSchemaJson).RootElement);

				try
				{
					Console.WriteLine("Asserting schema equality");
					Assert.AreEqual(onlineSchema, schema);

					Console.WriteLine("Validating local against online");
					onlineValidation.AssertValid();
					Console.WriteLine("Validating online against local");
					localValidation.AssertValid();

					Console.WriteLine("Asserting json equality");
					Assert.AreEqual(onlineSchemaJson, localSchemaJson);
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
		[Ignore("Schema equality not a feature yet")]
		public void RoundTrip(string testName, JsonSchema schema)
		{
			var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions{WriteIndented = true});
			Console.WriteLine(json);
			var returnTrip = JsonSerializer.Deserialize<JsonSchema>(json);

			Assert.AreEqual(schema, returnTrip);
		}
	}
}
