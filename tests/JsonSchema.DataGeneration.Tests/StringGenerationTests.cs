using System;
using NUnit.Framework;
using TestHelpers;
using static Json.Schema.DataGeneration.Tests.TestRunner;

namespace Json.Schema.DataGeneration.Tests;

public class StringGenerationTests
{
	[Test]
	public void SimpleString()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String);

		Run(schema);
	}

	[Test]
	public void MinLength()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.MinLength(30);

		Run(schema);
	}

	[Test]
	public void MaxLength()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.MaxLength(20);

		Run(schema);
	}

	[Test]
	public void SpecifiedRange()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.MinLength(10)
			.MaxLength(20);

		Run(schema);
	}

	[Test]
	public void ContainsDog()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern("dog");

		Run(schema);
	}

	[Test]
	public void ContainsDogWithSizeConstraints()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern("dog")
			.MinLength(10)
			.MaxLength(20);

		Run(schema);
	}

	[Test]
	public void DoesNotContainDog()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Not(new JsonSchemaBuilder().Pattern("dog"));

		Run(schema);
	}

	[Test]
	public void DoesNotContainDogWithSizeConstraints()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Not(new JsonSchemaBuilder().Pattern("dog"))
			.MinLength(10)
			.MaxLength(20);

		Run(schema);
	}

	[Test]
	public void ContainsCatAndDoesNotContainDogWithSizeConstraints()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Not(new JsonSchemaBuilder().Pattern("dog"))
			.Pattern("cat")
			.MinLength(10)
			.MaxLength(20);

		Run(schema);
	}

	[Test]
	public void ContainsEitherCatOrDog()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.AnyOf(
				new JsonSchemaBuilder().Pattern("dog"),
				new JsonSchemaBuilder().Pattern("cat")
			)
			.MinLength(10)
			.MaxLength(20);

		Run(schema);
	}

	[Test]
	public void ContainsExclusivelyEitherCatOrDog()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.OneOf(
				new JsonSchemaBuilder().Pattern("dog"),
				new JsonSchemaBuilder().Pattern("cat")
			)
			.MinLength(10)
			.MaxLength(20);

		Run(schema);
	}

	[Test]
	public void FormatDate()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Date);

		Run(schema);
	}

	[Test]
	public void FormatDateTime()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.DateTime);

		Run(schema);
	}

	[Test]
	public void FormatDuration()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Duration);

		Run(schema);
	}

	[Test]
	public void FormatEmail()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Email);

		Run(schema);
	}

	[Test]
	public void FormatHostname()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Hostname);

		Run(schema);
	}

	[Test]
	public void FormatIdnEmail()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.IdnEmail);

		Run(schema);
	}

	[Test]
	public void FormatIdnHostname()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.IdnHostname);

		Run(schema);
	}

	[Test]
	public void FormatIpv4()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Ipv4);

		Run(schema);
	}

	[Test]
	public void FormatIpv6()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Ipv6);

		Run(schema);
	}

	[Test]
	public void FormatIri()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Iri);

		Run(schema);
	}

	[Test]
	public void FormatIriReference()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder()
			.Type(SchemaValueType.String)
			.Format(Formats.IriReference);

		Run(schema);
	}

	[Test]
	public void FormatJsonPointer()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.JsonPointer);

		Run(schema);
	}

	[Test]
	public void FormatRelativeJsonPointer()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.RelativeJsonPointer);

		Run(schema);
	}

	[Test]
	public void FormatTime()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Time);

		Run(schema);
	}

	[Test]
	public void FormatUri()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Uri);

		Run(schema);
	}

	[Test]
	public void FormatUriReference()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.UriReference);

		Run(schema);
	}

	[Test]
	public void FormatUuid()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Format(Formats.Uuid);

		Run(schema);
	}

	[Test]
	public void MultipleFormats()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.AllOf(
				new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Format(Formats.DateTime),
				new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Format(Formats.Uuid)
			);

		var result = schema.GenerateData();

		TestConsole.WriteLine(result.ErrorMessage);
		Assert.That(result.IsSuccess, Is.False);
	}

	[Test]
	public void PatternDigitsOnly()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern(@"^\d+$");

		Run(schema);
	}

	[Test]
	public void PatternWordCharacters()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern(@"^\w{5,10}$");

		Run(schema);
	}

	[Test]
	public void PatternCharacterClass()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern(@"^[a-z]{3}$");

		Run(schema);
	}

	[Test]
	public void PatternAlternation()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern(@"^(foo|bar|baz)$");

		Run(schema);
	}

	[Test]
	public void PatternExplicitQuantifier()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern(@"^\d{4}-\d{2}-\d{2}$");

		Run(schema);
	}

	[Test]
	public void PatternWithOptionalGroup()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern(@"^https?://\S+$");

		Run(schema);
	}

	[Test]
	public void PatternNegatedCharacterClass()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern(@"^[^0-9]+$");

		Run(schema);
	}

	// ---- Pattern + length ----

	[Test]
	public void PatternWithMinLength()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern("cat")
			.MinLength(5);

		Run(schema);
	}

	[Test]
	public void PatternWithMinAndMaxLength()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern("cat")
			.MinLength(5)
			.MaxLength(15);

		Run(schema);
	}

	// ---- allOf: multiple required patterns (AND) ----

	[Test]
	public void AllOfTwoCompatiblePatterns()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.AllOf(
				new JsonSchemaBuilder().Pattern("cat"),
				new JsonSchemaBuilder().Pattern("hat")
			)
			.MinLength(6)
			.MaxLength(30);

		Run(schema);
	}

	[Test]
	public void AllOfPatternAndLength()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.AllOf(
				new JsonSchemaBuilder().Type(SchemaValueType.String).Pattern("foo").MinLength(5),
				new JsonSchemaBuilder().Type(SchemaValueType.String).MaxLength(20)
			);

		Run(schema);
	}

	[Test]
	public void AllOfWithSamePatternIsNotAConflict()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.AllOf(
				new JsonSchemaBuilder().Pattern("dog"),
				new JsonSchemaBuilder().Pattern("dog")
			);

		Run(schema);
	}

	// ---- not: anti-pattern (NOT) ----

	[Test]
	public void NotPatternDigits()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Not(new JsonSchemaBuilder().Pattern(@"\d"))
			.MinLength(5)
			.MaxLength(20);

		Run(schema);
	}

	[Test]
	public void NotPatternWithExplicitLiteral()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Not(new JsonSchemaBuilder().Pattern("forbidden"))
			.MinLength(5)
			.MaxLength(30);

		Run(schema);
	}

	// ---- Positive AND negative combined ----

	[Test]
	public void PatternRequiredAndForbiddenCombined()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern("cat")
			.Not(new JsonSchemaBuilder().Pattern("dog"))
			.MinLength(5)
			.MaxLength(30);

		Run(schema);
	}

	[Test]
	public void TwoRequiredAndOneForbiddenPattern()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.AllOf(
				new JsonSchemaBuilder().Pattern("cat"),
				new JsonSchemaBuilder().Pattern("hat")
			)
			.Not(new JsonSchemaBuilder().Pattern("dog"))
			.MinLength(6)
			.MaxLength(30);

		Run(schema);
	}

	// ---- anyOf / oneOf ----

	[Test]
	public void AnyOfPatternNoLengthConstraint()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.AnyOf(
				new JsonSchemaBuilder().Pattern("cat"),
				new JsonSchemaBuilder().Pattern("dog")
			);

		Run(schema);
	}

	[Test]
	public void OneOfPatternNoLengthConstraint()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.OneOf(
				new JsonSchemaBuilder().Pattern("cat"),
				new JsonSchemaBuilder().Pattern("dog")
			);

		Run(schema);
	}

	// ---- pattern + format ----

	[Test]
	public void PatternWithFormat()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern(@"\w+")
			.Format(Formats.Email);

		Run(schema);
	}

	// ---- pattern inside structural keywords ----

	[Test]
	public void PatternInsideArrayItems()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Array)
			.Items(new JsonSchemaBuilder()
				.Type(SchemaValueType.String)
				.Pattern("cat")
				.MinLength(3)
				.MaxLength(20))
			.MinItems(2)
			.MaxItems(5);

		Run(schema);
	}

	[Test]
	public void PatternOnObjectProperty()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.Object)
			.Properties(
				("name", new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Pattern(@"\w+")
					.MinLength(1)
					.MaxLength(30))
			)
			.Required("name");

		Run(schema);
	}

	// ---- failure cases ----

	[Test]
	public void ImpossiblePatternWithLengthFails()
	{
		var buildOptions = new BuildOptions { SchemaRegistry = new() };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Type(SchemaValueType.String)
			.Pattern(@"^\d{100}$")
			.MaxLength(5);

		RunFailure(schema);
	}
}