using System;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests;

/// <summary>
/// Tests that verify the system can build dialects dynamically from $vocabulary keywords in meta-schemas.
/// </summary>
public class VocabularyBasedDialectTests
{
	public class MinDateKeyword : IKeywordHandler
	{
		public static readonly MinDateKeyword Instance = new();

		public string Name => "minDate";

		private MinDateKeyword()
		{
		}

		public object? ValidateKeywordValue(JsonElement value)
		{
			if (value.ValueKind is not JsonValueKind.String)
				throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

			return DateTime.Parse(value.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
		}

		public void BuildSubschemas(KeywordData keyword, BuildContext context)
		{
		}

		public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
		{
			if (context.Instance.ValueKind is not JsonValueKind.String) return KeywordEvaluation.Ignore;

			var dateString = context.Instance.GetString();
			var date = DateTime.Parse(dateString!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
			var expectedDate = (DateTime)keyword.Value!;

			if (date < expectedDate)
			{
				return new KeywordEvaluation
				{
					Keyword = Name,
					IsValid = false,
					Error = $"Date must be on or after {expectedDate:O}"
				};
			}

			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};
		}
	}

	public class MaxDateKeyword : IKeywordHandler
	{
		public static readonly MaxDateKeyword Instance = new();

		public string Name => "maxDate";

		private MaxDateKeyword()
		{
		}

		public object? ValidateKeywordValue(JsonElement value)
		{
			if (value.ValueKind is not JsonValueKind.String)
				throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

			return DateTime.Parse(value.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
		}

		public void BuildSubschemas(KeywordData keyword, BuildContext context)
		{
		}

		public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
		{
			if (context.Instance.ValueKind is not JsonValueKind.String) return KeywordEvaluation.Ignore;

			var dateString = context.Instance.GetString();
			var date = DateTime.Parse(dateString!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
			var expectedDate = (DateTime)keyword.Value!;

			if (date > expectedDate)
			{
				return new KeywordEvaluation
				{
					Keyword = Name,
					IsValid = false,
					Error = $"Date must be on or before {expectedDate:O}"
				};
			}

			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};
		}
	}

	private static readonly JsonSerializerOptions _serializerOptions =
		new()
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

	// Define a custom vocabulary for date validation
	private static readonly Vocabulary _datesVocabulary =
		new(new Uri("https://json-everything.test/vocabulary/dates/v1"), MinDateKeyword.Instance, MaxDateKeyword.Instance);

	// Define a meta-schema builder that declares this vocabulary
	private static readonly JsonSchemaBuilder _datesMetaSchemaBuilder =
		new JsonSchemaBuilder()
			.Id("https://json-everything.test/meta/dates-draft-2019-09")
			.Schema(MetaSchemas.Draft201909Id)
			.Vocabulary(
				(Vocabulary.Draft201909_Core.Id, true),
				(Vocabulary.Draft201909_Applicator.Id, true),
				(Vocabulary.Draft201909_Validation.Id, true),
				(Vocabulary.Draft201909_MetaData.Id, true),
				(new Uri("https://json-everything.test/vocabulary/dates/v1"), true)
			)
			.Properties(
				(MinDateKeyword.Instance.Name, new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Format(Formats.DateTime)
				),
				(MaxDateKeyword.Instance.Name, new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Format(Formats.DateTime)
				)
			);

	[Test]
	public void VocabularyBasedDialect_BuildsFromMetaSchema_VocabularyRegistered()
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new(),
			VocabularyRegistry = new VocabularyRegistry(),
			DialectRegistry = new()
		};
		var datesMetaSchema = _datesMetaSchemaBuilder.Build(buildOptions);

		buildOptions.VocabularyRegistry.Register(_datesVocabulary);
		buildOptions.SchemaRegistry.Register(datesMetaSchema);

		var futureDate = DateTime.UtcNow.AddDays(1);
		var schema = new JsonSchemaBuilder(buildOptions)
			.Schema("https://json-everything.test/meta/dates-draft-2019-09")
			.MinDate(futureDate)
			.Build();

		var validInstance = JsonDocument.Parse($"\"{DateTime.UtcNow.AddDays(2):O}\"").RootElement;
		var invalidInstance = JsonDocument.Parse($"\"{DateTime.UtcNow:O}\"").RootElement;

		TestConsole.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
		TestConsole.WriteLine();

		var validResult = schema.Evaluate(validInstance);
		var invalidResult = schema.Evaluate(invalidInstance);

		validResult.AssertValid();
		invalidResult.AssertInvalid();
	}

	[Test]
	public void VocabularyBasedDialect_VocabularyNotRegistered_ThrowsException()
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new(),
			VocabularyRegistry = new VocabularyRegistry(),
			DialectRegistry = new()
		};
		var datesMetaSchema = _datesMetaSchemaBuilder.Build(buildOptions);

		// Don't register the vocabulary - it's required
		buildOptions.SchemaRegistry.Register(datesMetaSchema);

		var futureDate = DateTime.UtcNow.AddDays(1);

		Assert.Throws<JsonSchemaException>(() =>
			new JsonSchemaBuilder(buildOptions)
				.Schema("https://json-everything.test/meta/dates-draft-2019-09")
				.MinDate(futureDate)
				.Build());
	}

	[Test]
	public void VocabularyBasedDialect_OptionalVocabulary_WorksWithoutRegistration()
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new(),
			Dialect = Dialect.Draft201909,
			VocabularyRegistry = new VocabularyRegistry(),
			DialectRegistry = new()
		};

		// Create a meta-schema with an optional vocabulary
		var optionalVocabMetaSchema =
			new JsonSchemaBuilder()
				.Id("https://json-everything.test/meta/dates-optional")
				.Schema(MetaSchemas.Draft201909Id)
				.Vocabulary(
					(Vocabulary.Draft201909_Core.Id, true),
					(Vocabulary.Draft201909_Applicator.Id, true),
					(Vocabulary.Draft201909_Validation.Id, true),
					(new Uri("https://json-everything.test/vocabulary/dates/v1"), false) // Optional
				)
				.Build();

		// need a new build options
		buildOptions = new BuildOptions
		{
			SchemaRegistry = new(),
			Dialect = Dialect.Draft201909,
			VocabularyRegistry = new VocabularyRegistry(),
			DialectRegistry = new()
		};
		// Don't register the vocabulary - it's optional
		buildOptions.SchemaRegistry.Register(optionalVocabMetaSchema);

		var futureDate = DateTime.UtcNow.AddDays(1);
		var schema = new JsonSchemaBuilder(buildOptions)
			.Schema("https://json-everything.test/meta/dates-optional")
			.MinDate(futureDate)
			.Build();

		// Without the vocabulary registered, the minDate keyword should be ignored
		var invalidInstance = JsonDocument.Parse($"\"{DateTime.UtcNow:O}\"").RootElement;
		var result = schema.Evaluate(invalidInstance);

		result.AssertValid(); // Should pass because minDate is not processed
	}

	[Test]
	public void VocabularyBasedDialect_MetaSchemaValidation_VocabularyKnown()
	{
		var buildOptions = new BuildOptions
		{
			SchemaRegistry = new(),
			VocabularyRegistry = new VocabularyRegistry(),
			DialectRegistry = new()
		};
		var datesMetaSchema = _datesMetaSchemaBuilder.Build(buildOptions);

		buildOptions.VocabularyRegistry.Register(_datesVocabulary);
		buildOptions.SchemaRegistry.Register(datesMetaSchema);

		var futureDate = DateTime.UtcNow.AddDays(1);
		var schema = new JsonSchemaBuilder(buildOptions)
			.Schema("https://json-everything.test/meta/dates-draft-2019-09")
			.MinDate(futureDate)
			.Build();

		// Validate the schema against its meta-schema
		var schemaAsJson = JsonDocument.Parse(JsonSerializer.Serialize(schema, _serializerOptions)).RootElement;
		var results = datesMetaSchema.Evaluate(schemaAsJson);

		TestConsole.WriteLine(schemaAsJson);
		TestConsole.WriteLine();

		results.AssertValid();
	}
}
