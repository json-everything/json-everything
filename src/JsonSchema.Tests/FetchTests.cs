using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class FetchTests
{
	[Test]
	public void LocalRegistryFindsRef()
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new SchemaRegistry
			{
				Fetch = (uri, registry) =>
				{
					if (uri.AbsoluteUri == "https://json-everything.test/LocalRegistryFindsRef")
						return JsonSchema.FromText("{\"type\": \"string\"}", new BuildOptions { SchemaRegistry = registry });
					return null;
				}
			}
		};
		var schema = JsonSchema.FromText("{\"$ref\":\"https://json-everything.test/LocalRegistryFindsRef\"}", buildOptions);

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};

		using var json = JsonDocument.Parse("10");

		var results = schema.Evaluate(json.RootElement, options);

		results.AssertInvalid();
	}

	[Test]
	public void GlobalRegistryFindsRef()
	{
		try
		{
			SchemaRegistry.Global.Fetch = (uri, registry) =>
			{
				if (uri.AbsoluteUri == "https://json-everything.test/GlobalRegistryFindsRef")
					return JsonSchema.FromText("{\"type\": \"string\"}", new BuildOptions { SchemaRegistry = registry });
				return null;
			};

			var buildOptions = new BuildOptions
			{
				SchemaRegistry = SchemaRegistry.Global
			};
			var schema = JsonSchema.FromText("{\"$ref\":\"https://json-everything.test/GlobalRegistryFindsRef\"}", buildOptions);

			var options = new EvaluationOptions
			{
				OutputFormat = OutputFormat.Hierarchical
			};

			using var json = JsonDocument.Parse("10");

			var results = schema.Evaluate(json.RootElement, options);

			results.AssertInvalid();
		}
		finally
		{
			SchemaRegistry.Global.Fetch = null!;
		}
	}

	[Test]
	public void LocalRegistryMissesRef()
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new SchemaRegistry
			{
				Fetch = (uri, registry) =>
				{
					if (uri.AbsoluteUri == "https://json-everything.test/LocalRegistryMissesRef2")
						return JsonSchema.FromText("{\"type\": \"string\"}", new BuildOptions { SchemaRegistry = registry });
					return null;
				}
			}
		};
		var schema = JsonSchema.FromText("{\"$ref\":\"https://json-everything.test/LocalRegistryMissesRef1\"}", buildOptions);

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.Hierarchical
		};

		using var json = JsonDocument.Parse("10");

		Assert.Throws<RefResolutionException>(() => schema.Evaluate(json.RootElement, options));
	}

	[Test]
	public void GlobalRegistryMissesRef()
	{
		try
		{
			SchemaRegistry.Global.Fetch = (uri, registry) =>
			{
				if (uri.AbsoluteUri == "https://json-everything.test/GlobalRegistryMissesRef2")
					return JsonSchema.FromText("{\"type\": \"string\"}", new BuildOptions { SchemaRegistry = registry });
				return null;
			};
			var schema = JsonSchema.FromText("{\"$ref\":\"https://json-everything.test/GlobalRegistryMissesRef1\"}");

			var options = new EvaluationOptions
			{
				OutputFormat = OutputFormat.Hierarchical
			};

			using var json = JsonDocument.Parse("10");

			Assert.Throws<RefResolutionException>(() => schema.Evaluate(json.RootElement, options));
		}
		finally
		{
			SchemaRegistry.Global.Fetch = null!;
		}
	}

	[Test]
	public void RefContainsBadJson()
	{
		try
		{
			SchemaRegistry.Global.Fetch = (_, registry) => JsonSchema.FromText("{\"type\": \"string\", \"invalid\"}", new BuildOptions { SchemaRegistry = registry });

			var buildOptions = new BuildOptions
			{
				SchemaRegistry = SchemaRegistry.Global
			};

			Assert.Catch<JsonException>(() => JsonSchema.FromText("{\"$ref\":\"https://json-everything.test/RefContainsBadJson\"}", buildOptions));
		}
		finally
		{
			SchemaRegistry.Global.Fetch = null!;
		}
	}
}