using System.Text.Json;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class FormatRegistryExtensionsTests
{
	private class RequiredCharacterFormat : Format
	{
		private readonly char _required;

		public RequiredCharacterFormat(string key, char required)
			: base(key)
		{
			_required = required;
		}

		public override bool Validate(JsonElement value, out string? errorMessage)
		{
			errorMessage = null;
			if (value.ValueKind != JsonValueKind.String) return true;

			if (value.GetString()!.IndexOf(_required) >= 0) return true;

			errorMessage = $"Value must contain '{_required}'";
			return false;
		}
	}

	private static EvaluationResults Evaluate(FormatRegistry registry, string format, string instanceText)
	{
		var schemaText =
			$$"""
			{
			  "type": "string",
			  "format": "{{format}}"
			}
			""";
		var buildOptions = new BuildOptions
		{
			Dialect = Dialect.Draft202012.With(formatRegistry: registry)
		};
		var schema = JsonSchema.FromText(schemaText, buildOptions);
		var instance = JsonDocument.Parse(instanceText).RootElement;

		return schema.Evaluate(instance, new EvaluationOptions { RequireFormatValidation = true });
	}

	[Test]
	public void WithAddsCustomFormat_Passes()
	{
		var registry = FormatRegistry.Draft202012.With(new RequiredCharacterFormat("needs-dash", '-'));

		var results = Evaluate(registry, "needs-dash", "\"has-a-dash\"");

		results.AssertValid();
	}

	[Test]
	public void WithAddsCustomFormat_Fails()
	{
		var registry = FormatRegistry.Draft202012.With(new RequiredCharacterFormat("needs-dash", '-'));

		var results = Evaluate(registry, "needs-dash", "\"nodash\"");

		results.AssertInvalid();
	}

	[Test]
	public void WithRetainsKnownFormats()
	{
		var registry = FormatRegistry.Draft202012.With(new RequiredCharacterFormat("needs-dash", '-'));

		var results = Evaluate(registry, "email", "\"not an email\"");

		results.AssertInvalid();
	}

	[Test]
	public void WithOverridesKnownFormat_Passes()
	{
		var registry = FormatRegistry.Draft202012.With(new RequiredCharacterFormat("email", '@'));

		var results = Evaluate(registry, "email", "\"@\"");

		results.AssertValid();
	}

	[Test]
	public void WithOverridesKnownFormat_Fails()
	{
		var registry = FormatRegistry.Draft202012.With(new RequiredCharacterFormat("email", '@'));

		var results = Evaluate(registry, "email", "\"user.example.com\"");

		results.AssertInvalid();
	}

	[Test]
	public void WithDoesNotModifySourceRegistry()
	{
		_ = FormatRegistry.Draft202012.With(new RequiredCharacterFormat("needs-dash", '-'));

		var results = Evaluate(FormatRegistry.Draft202012, "needs-dash", "\"nodash\"");

		results.AssertValid();
	}

	[Test]
	public void WithoutKeyRemovesCustomFormat()
	{
		var registry = FormatRegistry.Draft202012
			.With(new RequiredCharacterFormat("needs-dash", '-'))
			.Without("needs-dash");

		var results = Evaluate(registry, "needs-dash", "\"nodash\"");

		results.AssertValid();
	}

	[Test]
	public void WithoutKeyRemovesKnownFormatOverride()
	{
		var registry = FormatRegistry.Draft202012
			.With(new RequiredCharacterFormat("email", '@'))
			.Without("email");

		var results = Evaluate(registry, "email", "\"@\"");

		results.AssertInvalid();
	}

	[Test]
	public void WithoutKeyRetainsOtherFormats()
	{
		var registry = FormatRegistry.Draft202012
			.With(new RequiredCharacterFormat("needs-dash", '-'))
			.Without("email");

		var results = Evaluate(registry, "needs-dash", "\"nodash\"");

		results.AssertInvalid();
	}

	[Test]
	public void WithoutFormatRemovesSameInstance()
	{
		var format = new RequiredCharacterFormat("needs-dash", '-');
		var registry = FormatRegistry.Draft202012
			.With(format)
			.Without(format);

		var results = Evaluate(registry, "needs-dash", "\"nodash\"");

		results.AssertValid();
	}

	[Test]
	public void WithoutFormatRemovesByKey()
	{
		var registry = FormatRegistry.Draft202012
			.With(new RequiredCharacterFormat("email", '@'))
			.Without(new RequiredCharacterFormat("email", '@'));

		var results = Evaluate(registry, "email", "\"@\"");

		results.AssertInvalid();
	}
}
