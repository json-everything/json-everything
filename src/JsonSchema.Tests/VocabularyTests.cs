﻿using System;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests;

public partial class VocabularyTests
{
	[SchemaKeyword(Name)]
	[SchemaSpecVersion(SpecVersion.Draft201909 | SpecVersion.Draft202012)]
	[JsonConverter(typeof(MinDateJsonConverter))]
	[Vocabulary("http://mydates.com/vocabulary")]
	public class MinDateKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "minDate";

		public DateTime Date { get; }

		public MinDateKeyword(DateTime date)
		{
			Date = date;
		}

		public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
			ReadOnlySpan<KeywordConstraint> localConstraints,
			EvaluationContext context)
		{
			return new KeywordConstraint(Name, (e, _) =>
			{
				var dateString = e.LocalInstance!.GetValue<string>();
				var date = DateTime.Parse(dateString);

				if (date < Date)
					e.Results.Fail(Name, "[[provided:O]] must be on or after [[value:O]]"
						.ReplaceToken("provided", date, VocabularySerializerContext.Default.DateTime)
						.ReplaceToken("value", Date, VocabularySerializerContext.Default.DateTime));

			});
		}
	}

	public class MinDateJsonConverter : WeaklyTypedJsonConverter<MinDateKeyword>
	{
		public override MinDateKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var dateString = reader.GetString();
			var date = DateTime.Parse(dateString!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
			return new MinDateKeyword(date);
		}

		public override void Write(Utf8JsonWriter writer, MinDateKeyword value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK"));
		}
	}

	[SchemaKeyword(Name)]
	[SchemaSpecVersion(SpecVersion.Draft7 | SpecVersion.Draft201909 | SpecVersion.Draft202012)]
	[JsonConverter(typeof(NonVocabMinDateJsonConverter))]
	public class NonVocabMinDateKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "minDate-nv";

		public DateTime Date { get; }

		public NonVocabMinDateKeyword(DateTime date)
		{
			Date = date;
		}

		public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
			ReadOnlySpan<KeywordConstraint> localConstraints,
			EvaluationContext context)
		{
			return new KeywordConstraint(Name, (e, _) =>
			{
				var dateString = e.LocalInstance!.GetValue<string>();
				var date = DateTime.Parse(dateString);

				if (date < Date)
					e.Results.Fail(Name, "[[provided:O]] must be on or after [[value:O]]"
						.ReplaceToken("provided", date, VocabularySerializerContext.Default.DateTime)
						.ReplaceToken("value", Date, VocabularySerializerContext.Default.DateTime));

			});
		}
	}

	public class NonVocabMinDateJsonConverter : WeaklyTypedJsonConverter<NonVocabMinDateKeyword>
	{
		public override NonVocabMinDateKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var dateString = reader.GetString();
			var date = DateTime.Parse(dateString!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
			return new NonVocabMinDateKeyword(date);
		}

		public override void Write(Utf8JsonWriter writer, NonVocabMinDateKeyword value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK"));
		}
	}

	[SchemaKeyword(Name)]
	[SchemaSpecVersion(SpecVersion.Draft201909 | SpecVersion.Draft202012)]
	[JsonConverter(typeof(MaxDateJsonConverter))]
	[Vocabulary("http://mydates.com/vocabulary")]
	public class MaxDateKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "maxDate";

		public DateTime Date { get; }

		public MaxDateKeyword(DateTime date)
		{
			Date = date;
		}

		public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
			ReadOnlySpan<KeywordConstraint> localConstraints,
			EvaluationContext context)
		{
			throw new NotImplementedException();
		}
	}

	public class MaxDateJsonConverter : WeaklyTypedJsonConverter<MaxDateKeyword>
	{
		public override MaxDateKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.String)
				throw new JsonException("Expected string");

			var dateString = reader.GetString();
			var date = DateTime.Parse(dateString!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
			return new MaxDateKeyword(date);
		}

		public override void Write(Utf8JsonWriter writer, MaxDateKeyword value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK"));
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
		new("http://mydates.com/vocabulary", typeof(MinDateKeyword), typeof(MaxDateKeyword));

	public static readonly JsonSchema DatesMetaSchema =
		new JsonSchemaBuilder()
			.Id("http://mydates.com/schema")
			.Schema(MetaSchemas.Draft201909Id)
			.Vocabulary(
				(Vocabularies.Core201909Id, true),
				("http://mydates.com/vocabulary", true)
			)
			.Properties(
				(MinDateKeyword.Name, new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Format(Formats.DateTime)
				),
				(MaxDateKeyword.Name, new JsonSchemaBuilder()
					.Type(SchemaValueType.String)
					.Format(Formats.DateTime)
				)
			);

	[OneTimeSetUp]
	public void Setup()
	{
		SchemaKeywordRegistry.Register<MinDateKeyword>(VocabularySerializerContext.Default);
		SchemaKeywordRegistry.Register<NonVocabMinDateKeyword>(VocabularySerializerContext.Default);
		SchemaKeywordRegistry.Register<MaxDateKeyword>(VocabularySerializerContext.Default);
	}

	[OneTimeTearDown]
	public void TearDown()
	{
		SchemaKeywordRegistry.Unregister<MinDateKeyword>();
		SchemaKeywordRegistry.Unregister<NonVocabMinDateKeyword>();
		SchemaKeywordRegistry.Unregister<MaxDateKeyword>();
	}

	[Test]
	public void SchemaValidation_ValidateMetaSchemaTrue_VocabularyNotKnown()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema("http://mydates.com/schema")
			.MinDate(DateTime.Now.ToUniversalTime().AddDays(-1));
		var instance = JsonNode.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"");

		var options = new EvaluationOptions
		{
			ValidateAgainstMetaSchema = true,
			OutputFormat = OutputFormat.List
		};
		options.SchemaRegistry.Register(DatesMetaSchema);

		TestConsole.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
		TestConsole.WriteLine();
		TestConsole.WriteLine(instance);
		TestConsole.WriteLine();

		Assert.Throws<JsonSchemaException>(() => schema.Evaluate(instance, options));
	}

	[Test]
	public void SchemaValidation_ValidateMetaSchemaFalse_VocabularyNotKnown()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema("http://mydates.com/schema")
			.MinDate(DateTime.Now.ToUniversalTime().AddDays(-1));
		var instance = JsonNode.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"");

		var options = new EvaluationOptions();
		options.SchemaRegistry.Register(DatesMetaSchema);
		var results = schema.Evaluate(instance, options);

		TestConsole.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
		TestConsole.WriteLine();
		TestConsole.WriteLine(instance);
		TestConsole.WriteLine();

		results.AssertValid();
	}

	[Test]
	public void SchemaValidation_ValidateMetaSchemaFalse_NonVocab_Draft201909_NoCustomKeywords()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft201909Id)
			.NonVocabMinDate(DateTime.Now.ToUniversalTime().AddDays(1));
		var instance = JsonNode.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"");

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
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft201909Id)
			.NonVocabMinDate(DateTime.Now.ToUniversalTime().AddDays(1));
		var instance = JsonNode.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"");

		var results = schema.Evaluate(instance, new EvaluationOptions
		{
			ProcessCustomKeywords = true,
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
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft7Id)
			.NonVocabMinDate(DateTime.Now.ToUniversalTime().AddDays(1));
		var instance = JsonNode.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"");

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
		try
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Schema("http://mydates.com/schema")
				.MinDate(DateTime.Now.ToUniversalTime().AddDays(-1));
			var instance = JsonNode.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"");

			var options = new EvaluationOptions
			{
				ValidateAgainstMetaSchema = true,
				OutputFormat = OutputFormat.List
			};
			options.SchemaRegistry.Register(DatesMetaSchema);
			VocabularyRegistry.Register(DatesVocabulary);
			var results = schema.Evaluate(instance, options);

			TestConsole.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
			TestConsole.WriteLine();
			TestConsole.WriteLine(instance);
			TestConsole.WriteLine();

			results.AssertValid();
		}
		finally
		{
			VocabularyRegistry.Unregister(DatesVocabulary);
		}
	}

	[Test]
	public void MetaSchemaValidation_VocabularyNotKnown()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema("http://mydates.com/schema")
			.MinDate(DateTime.Now.AddDays(-1));

		var schemaAsJson = JsonNode.Parse(JsonSerializer.Serialize(schema, _basicOptions));
		var results = DatesMetaSchema.Evaluate(schemaAsJson, new EvaluationOptions{OutputFormat = OutputFormat.List});

		TestConsole.WriteLine(schemaAsJson);
		TestConsole.WriteLine();

		results.AssertInvalid();
	}

	[Test]
	public void MetaSchemaValidation_VocabularyKnown()
	{
		try
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Schema("http://mydates.com/schema")
				.MinDate(DateTime.Now.AddDays(-1));

			var schemaAsJson = JsonNode.Parse(JsonSerializer.Serialize(schema, _basicOptions));
			var options = new EvaluationOptions();
			VocabularyRegistry.Register(DatesVocabulary);
			var results = DatesMetaSchema.Evaluate(schemaAsJson, options);

			TestConsole.WriteLine(schemaAsJson);
			TestConsole.WriteLine();

			results.AssertValid();
		}
		finally
		{
			VocabularyRegistry.Unregister(DatesVocabulary);
		}
	}

	[SchemaKeyword(Name)]
	[SchemaSpecVersion(SpecVersion.Draft202012)]
	[JsonConverter(typeof(Draft4ExclusiveMinimumJsonConverter))]
	public class Draft4ExclusiveMinimumKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "exclusiveMinimum";

		private readonly ExclusiveMinimumKeyword? _postDraft6Keyword;

		public bool? BoolValue { get; }
		public decimal? NumberValue => _postDraft6Keyword?.Value;

		public Draft4ExclusiveMinimumKeyword(bool value)
		{
			BoolValue = value;
		}

		public Draft4ExclusiveMinimumKeyword(decimal value)
		{
			_postDraft6Keyword = new ExclusiveMinimumKeyword(value);
		}

		public KeywordConstraint GetConstraint(SchemaConstraint schemaConstraint,
			ReadOnlySpan<KeywordConstraint> localConstraints,
			EvaluationContext context)
		{
			if (BoolValue.HasValue)
			{
				var minimumConstraint = localConstraints.GetKeywordConstraint<MinimumKeyword>();

				var constraint = new KeywordConstraint(Name, (e, _) =>
				{
					if (!BoolValue.Value) return;

					var minimum = (decimal?) 5;// context.LocalSchema.GetMinimum();

					if (!minimum.HasValue) return;

					var schemaValueType = e.LocalInstance.GetSchemaValueType();
					if (schemaValueType is not (SchemaValueType.Number or SchemaValueType.Integer)) return;

					var number = e.LocalInstance!.AsValue().GetNumber();
					if (number == minimum)
						e.Results.Fail(Name, "minimum is exclusive");
				});
				if (minimumConstraint != null)
					constraint.SiblingDependencies = [minimumConstraint];
				return constraint;
			}

			return _postDraft6Keyword!.GetConstraint(schemaConstraint, localConstraints, context);
		}
	}

	public class Draft4ExclusiveMinimumJsonConverter : WeaklyTypedJsonConverter<Draft4ExclusiveMinimumKeyword>
	{
		public override Draft4ExclusiveMinimumKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			return reader.TokenType switch
			{
				JsonTokenType.True or JsonTokenType.False => new Draft4ExclusiveMinimumKeyword(reader.GetBoolean()),
				JsonTokenType.Number => new Draft4ExclusiveMinimumKeyword(reader.GetDecimal()),
				_ => throw new JsonException("Expected boolean or number")
			};
		}

		public override void Write(Utf8JsonWriter writer, Draft4ExclusiveMinimumKeyword value, JsonSerializerOptions options)
		{
			if (value.BoolValue.HasValue)
				writer.WriteBoolean(Draft4ExclusiveMinimumKeyword.Name, value.BoolValue.Value);
			else
				writer.WriteNumber(Draft4ExclusiveMinimumKeyword.Name, value.NumberValue!.Value);
		}
	}

	[TestCase(3, false)]
	[TestCase(8, true)]
	[TestCase(5, false)]
	[TestCase(5.1, true)]
	public void Draft4ExclusiveMinimumOverride(decimal instanceValue, bool isValid)
	{
		try
		{
			SchemaKeywordRegistry.Register<Draft4ExclusiveMinimumKeyword>(VocabularySerializerContext.Default);

			var schemaText = @"{
	""minimum"": 5,
	""exclusiveMinimum"": true
}";
			var schema = JsonSchema.FromText(schemaText);

			JsonNode instance = instanceValue;

			var result = schema.Evaluate(instance);

			Assert.That(result.IsValid, Is.EqualTo(isValid));
		}
		finally
		{
			SchemaKeywordRegistry.Register<ExclusiveMinimumKeyword>();
		}
	}

	[TestCase(3, false)]
	[TestCase(8, true)]
	[TestCase(5, false)]
	[TestCase(5.1, true)]
	public void Draft4ExclusiveMinimumOverrideWithDraft6Usage(decimal instanceValue, bool isValid)
	{
		try
		{
			SchemaKeywordRegistry.Register<Draft4ExclusiveMinimumKeyword>(VocabularySerializerContext.Default);

			var schemaText = @"{
	""exclusiveMinimum"": 5
}";
			var schema = JsonSchema.FromText(schemaText);

			JsonNode instance = instanceValue;

			var result = schema.Evaluate(instance);

			Assert.That(result.IsValid, Is.EqualTo(isValid));
		}
		finally
		{
			SchemaKeywordRegistry.Register<ExclusiveMinimumKeyword>();
		}
	}

	[JsonSerializable(typeof(Draft4ExclusiveMinimumKeyword))]
	[JsonSerializable(typeof(MinDateKeyword))]
	[JsonSerializable(typeof(NonVocabMinDateKeyword))]
	[JsonSerializable(typeof(MaxDateKeyword))]
	[JsonSerializable(typeof(DateTime))]
	internal partial class VocabularySerializerContext : JsonSerializerContext;
}