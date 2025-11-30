using System;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests;

public partial class VocabularyTests
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
					Error = "[[provided:O]] must be on or after [[value:O]]"
						.ReplaceToken("provided", date, VocabularySerializerContext.Default.DateTime)
						.ReplaceToken("value", expectedDate, VocabularySerializerContext.Default.DateTime)
				};
			}

			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};
		}
	}

	public class NonVocabMinDateKeyword : IKeywordHandler
	{
		public static readonly NonVocabMinDateKeyword Instance = new();

		public string Name => "minDate-nv";

		private NonVocabMinDateKeyword()
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
					Error = "[[provided:O]] must be on or after [[value:O]]"
						.ReplaceToken("provided", date, VocabularySerializerContext.Default.DateTime)
						.ReplaceToken("value", expectedDate, VocabularySerializerContext.Default.DateTime)
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
			throw new NotImplementedException();
		}
	}

	private static readonly JsonSerializerOptions _basicOptions =
		new()
		{
			TypeInfoResolverChain = { VocabularySerializerContext.Default, TestSerializerContext.Default }
		};

	private static readonly JsonSerializerOptions _serializerOptions =
		new()
		{
			TypeInfoResolverChain = { VocabularySerializerContext.Default, TestSerializerContext.Default },
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

	public static readonly Vocabulary DatesVocabulary =
		new(new Uri("http://mydates.com/vocabulary"), MinDateKeyword.Instance, MaxDateKeyword.Instance);

	public static readonly JsonSchema DatesMetaSchema =
		new JsonSchemaBuilder()
			.Id("http://mydates.com/schema")
			.Schema(MetaSchemas.Draft201909Id)
			.Vocabulary(
				(Vocabulary.Draft201909_Core.Id, true),
				(new Uri("http://mydates.com/vocabulary"), true)
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

	private static readonly Dialect DatesDialect = Dialect.Draft201909.With(
		[MinDateKeyword.Instance, MaxDateKeyword.Instance],
		id: new Uri("http://mydates.com/schema"),
		allowUnknownKeywords: true
	);

	private static readonly Dialect NonVocabDialect201909 = Dialect.Draft201909.With(
		[NonVocabMinDateKeyword.Instance],
		allowUnknownKeywords: true
	);

	private static readonly Dialect NonVocabDialect07 = Dialect.Draft07.With(
		[NonVocabMinDateKeyword.Instance],
		allowUnknownKeywords: true
	);

	[Test]
	public void SchemaValidation_ValidateMetaSchemaTrue_VocabularyNotKnown()
	{
		var dialectRegistry = new DialectRegistry();
		dialectRegistry.Register(DatesDialect);

		var buildOptions = new BuildOptions { DialectRegistry = dialectRegistry };
		buildOptions.SchemaRegistry.Register(DatesMetaSchema);

		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema("http://mydates.com/schema")
			.MinDate(DateTime.Now.ToUniversalTime().AddDays(-1));
		var instance = JsonDocument.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"").RootElement;

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.List
		};

		TestConsole.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
		TestConsole.WriteLine();
		TestConsole.WriteLine(instance);
		TestConsole.WriteLine();

		Assert.Throws<JsonSchemaException>(() => schema.Evaluate(instance, options));
	}

	[Test]
	public void SchemaValidation_ValidateMetaSchemaFalse_VocabularyNotKnown()
	{
		var dialectRegistry = new DialectRegistry();
		dialectRegistry.Register(DatesDialect);

		var buildOptions = new BuildOptions { DialectRegistry = dialectRegistry };
		buildOptions.SchemaRegistry.Register(DatesMetaSchema);

		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema("http://mydates.com/schema")
			.MinDate(DateTime.Now.ToUniversalTime().AddDays(-1));
		var instance = JsonDocument.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"").RootElement;

		var results = schema.Evaluate(instance);

		TestConsole.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
		TestConsole.WriteLine();
		TestConsole.WriteLine(instance);
		TestConsole.WriteLine();

		results.AssertValid();
	}

	[Test]
	public void SchemaValidation_ValidateMetaSchemaFalse_NonVocab_Draft201909_NoCustomKeywords()
	{
		var buildOptions = new BuildOptions { Dialect = Dialect.Draft201909 };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.Draft201909Id)
			.NonVocabMinDate(DateTime.Now.ToUniversalTime().AddDays(1));
		var instance = JsonDocument.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"").RootElement;

		var results = schema.Evaluate(instance);

		TestConsole.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
		TestConsole.WriteLine();
		TestConsole.WriteLine(instance);
		TestConsole.WriteLine();

		results.AssertValid();
	}

	[Test]
	public void SchemaValidation_ValidateMetaSchemaFalse_NonVocab_Draft201909_WithCustomKeywords()
	{
		var buildOptions = new BuildOptions { Dialect = NonVocabDialect201909 };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.Draft201909Id)
			.NonVocabMinDate(DateTime.Now.ToUniversalTime().AddDays(1));
		var instance = JsonDocument.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"").RootElement;

		var results = schema.Evaluate(instance, new EvaluationOptions
		{
			OutputFormat = OutputFormat.List
		});

		TestConsole.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
		TestConsole.WriteLine();
		TestConsole.WriteLine(instance);
		TestConsole.WriteLine();

		results.AssertInvalid();
	}

	[Test]
	public void SchemaValidation_ValidateMetaSchemaFalse_NonVocab_Draft7()
	{
		var buildOptions = new BuildOptions { Dialect = NonVocabDialect07 };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema(MetaSchemas.Draft7Id)
			.NonVocabMinDate(DateTime.Now.ToUniversalTime().AddDays(1));
		var instance = JsonDocument.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"").RootElement;

		var results = schema.Evaluate(instance);

		TestConsole.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
		TestConsole.WriteLine();
		TestConsole.WriteLine(instance);
		TestConsole.WriteLine();

		results.AssertInvalid();
	}

	[Test]
	public void SchemaValidation_ValidateMetaSchemaTrue_VocabularyKnown()
	{
		var dialectRegistry = new DialectRegistry();
		dialectRegistry.Register(DatesDialect);

		var buildOptions = new BuildOptions
		{
			DialectRegistry = dialectRegistry,
			VocabularyRegistry = new VocabularyRegistry()
		};
		buildOptions.VocabularyRegistry.Register(DatesVocabulary);
		buildOptions.SchemaRegistry.Register(DatesMetaSchema);

		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema("http://mydates.com/schema")
			.MinDate(DateTime.Now.ToUniversalTime().AddDays(-1));
		var instance = JsonDocument.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"").RootElement;

		var options = new EvaluationOptions
		{
			OutputFormat = OutputFormat.List
		};
		var results = schema.Evaluate(instance, options);

		TestConsole.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
		TestConsole.WriteLine();
		TestConsole.WriteLine(instance);
		TestConsole.WriteLine();

		results.AssertValid();
	}

	[Test]
	public void MetaSchemaValidation_VocabularyNotKnown()
	{
		var dialectRegistry = new DialectRegistry();
		dialectRegistry.Register(DatesDialect);

		var buildOptions = new BuildOptions { DialectRegistry = dialectRegistry };
		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema("http://mydates.com/schema")
			.MinDate(DateTime.Now.AddDays(-1));

		var schemaAsJson = JsonDocument.Parse(JsonSerializer.Serialize(schema, _basicOptions)).RootElement;
		var results = DatesMetaSchema.Evaluate(schemaAsJson, new EvaluationOptions{OutputFormat = OutputFormat.List});

		TestConsole.WriteLine(schemaAsJson);
		TestConsole.WriteLine();

		results.AssertInvalid();
	}

	[Test]
	public void MetaSchemaValidation_VocabularyKnown()
	{
		var dialectRegistry = new DialectRegistry();
		dialectRegistry.Register(DatesDialect);

		var buildOptions = new BuildOptions
		{
			DialectRegistry = dialectRegistry,
			VocabularyRegistry = new VocabularyRegistry()
		};
		buildOptions.VocabularyRegistry.Register(DatesVocabulary);

		JsonSchema schema = new JsonSchemaBuilder(buildOptions)
			.Schema("http://mydates.com/schema")
			.MinDate(DateTime.Now.AddDays(-1));

		var schemaAsJson = JsonDocument.Parse(JsonSerializer.Serialize(schema, _basicOptions)).RootElement;
		var results = DatesMetaSchema.Evaluate(schemaAsJson);

		TestConsole.WriteLine(schemaAsJson);
		TestConsole.WriteLine();

		results.AssertValid();
	}

	public class Draft4ExclusiveMinimumKeyword : IKeywordHandler
	{
		public static readonly Draft4ExclusiveMinimumKeyword Instance = new();

		public string Name => "exclusiveMinimum";

		private Draft4ExclusiveMinimumKeyword()
		{
		}

		public object? ValidateKeywordValue(JsonElement value)
		{
			if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
				throw new JsonSchemaException($"'{Name}' value must be a boolean, found {value.ValueKind}");

			return value.GetBoolean();
		}

		public void BuildSubschemas(KeywordData keyword, BuildContext context)
		{
			// Store the minimum value if present in sibling keywords
			if (context.LocalSchema.TryGetProperty("minimum", out var minimumValue))
			{
				keyword.Value = (keyword.Value, minimumValue);
			}
		}

		public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
		{
			if (context.Instance.ValueKind is not JsonValueKind.Number) return KeywordEvaluation.Ignore;

			var (isExclusive, minimum) = ((bool, JsonElement))keyword.Value!;
			if (!isExclusive)
			{
				return new KeywordEvaluation
				{
					Keyword = Name,
					IsValid = true
				};
			}

			var comparison = JsonMath.NumberCompare(context.Instance, minimum);
			if (comparison == 0)
			{
				return new KeywordEvaluation
				{
					Keyword = Name,
					IsValid = false,
					Error = "minimum is exclusive"
				};
			}

			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};
		}
	}
	
	[TestCase(3, false)]
	[TestCase(8, true)]
	[TestCase(5, false)]
	[TestCase(5.1, true)]
	public void Draft4ExclusiveMinimumOverride(decimal instanceValue, bool isValid)
	{
		var draft4Dialect = Dialect.Draft202012.With([Draft4ExclusiveMinimumKeyword.Instance]);

		var schemaText = @"{
	""minimum"": 5,
	""exclusiveMinimum"": true
}";
		var schema = JsonSchema.FromText(schemaText, new BuildOptions { Dialect = draft4Dialect });

		var instance = JsonDocument.Parse(instanceValue.ToString(System.Globalization.CultureInfo.InvariantCulture)).RootElement;

		var result = schema.Evaluate(instance);

		Assert.That(result.IsValid, Is.EqualTo(isValid));
	}

	[JsonSerializable(typeof(Draft4ExclusiveMinimumKeyword))]
	[JsonSerializable(typeof(MinDateKeyword))]
	[JsonSerializable(typeof(NonVocabMinDateKeyword))]
	[JsonSerializable(typeof(MaxDateKeyword))]
	[JsonSerializable(typeof(DateTime))]
	internal partial class VocabularySerializerContext : JsonSerializerContext;
}