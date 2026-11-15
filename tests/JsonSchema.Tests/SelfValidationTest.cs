using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using Json.More;
using NUnit.Framework;
using TestHelpers;

// ReSharper disable LocalizableElement

namespace Json.Schema.Tests;

[TestFixture]
public class SelfValidationTest
{
	public static IEnumerable<TestCaseData> TestData =>
		[
			new(MetaSchemas.Draft6) { TestName = nameof(MetaSchemas.Draft6) },

			new(MetaSchemas.Draft7) { TestName = nameof(MetaSchemas.Draft7) },

			new(MetaSchemas.Draft201909) { TestName = nameof(MetaSchemas.Draft201909) },
			new(MetaSchemas.Core201909) { TestName = nameof(MetaSchemas.Core201909) },
			new(MetaSchemas.Applicator201909) { TestName = nameof(MetaSchemas.Applicator201909) },
			new(MetaSchemas.Validation201909) { TestName = nameof(MetaSchemas.Validation201909) },
			new(MetaSchemas.Metadata201909) { TestName = nameof(MetaSchemas.Metadata201909) },
			new(MetaSchemas.Format201909) { TestName = nameof(MetaSchemas.Format201909) },
			new(MetaSchemas.Content201909) { TestName = nameof(MetaSchemas.Content201909) },

			new(MetaSchemas.Draft202012) { TestName = nameof(MetaSchemas.Draft202012) },
			new(MetaSchemas.Core202012) { TestName = nameof(MetaSchemas.Core202012) },
			new(MetaSchemas.Applicator202012) { TestName = nameof(MetaSchemas.Applicator202012) },
			new(MetaSchemas.Metadata202012) { TestName = nameof(MetaSchemas.Metadata202012) },
			new(MetaSchemas.FormatAnnotation202012) { TestName = nameof(MetaSchemas.FormatAnnotation202012) },
			new(MetaSchemas.FormatAssertion202012) { TestName = nameof(MetaSchemas.FormatAssertion202012) },
			new(MetaSchemas.Content202012) { TestName = nameof(MetaSchemas.Content202012) },
			new(MetaSchemas.Unevaluated202012) { TestName = nameof(MetaSchemas.Unevaluated202012) },
		];

	[TestCaseSource(nameof(TestData))]
	public void Hardcoded(JsonSchema schema)
	{
		var validation = schema.Evaluate(schema.Root.Source, new EvaluationOptions { OutputFormat = OutputFormat.Hierarchical });

		validation.AssertValid();
	}

	[TestCaseSource(nameof(TestData))]
	public void Online(JsonSchema schema)
	{
		try
		{
			var buildOptions = new BuildOptions
			{
				SchemaRegistry = new()
			};

			var onlineSchemaJson = new HttpClient().GetStringAsync(schema.Root.Keywords.FirstOrDefault(x => x.Handler.Name == "$id")!.RawValue.GetString()!).Result;
			var onlineSchema = JsonSchema.FromText(onlineSchemaJson, buildOptions);

			var onlineInstance = JsonDocument.Parse(onlineSchemaJson).RootElement;
			var localInstance = schema.Root.Source;
			var localValidation = schema.Evaluate(onlineInstance);
			var onlineValidation = onlineSchema.Evaluate(localInstance);

			try
			{
				TestConsole.WriteLine("Asserting schema equality");
				var asElement = schema.Root.Source;
				var onlineAsElement = onlineSchema.Root.Source;
				Assert.That(() => asElement.IsEquivalentTo(onlineAsElement));

				TestConsole.WriteLine("Validating local against online");
				onlineValidation.AssertValid();
				TestConsole.WriteLine("Validating online against local");
				localValidation.AssertValid();
			}
			catch (Exception)
			{
				TestConsole.WriteLine("Online {0}", onlineSchemaJson);
				TestConsole.WriteLine("Local {0}", schema.Root.Source);
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

}