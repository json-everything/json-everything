using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Pointer;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class VocabularyTests
{
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909 | Draft.Draft202012)]
	[JsonConverter(typeof(MinDateJsonConverter))]
	[Vocabulary("http://mydates.com/vocabulary")]
	public class MinDateKeyword : IJsonSchemaKeyword, IEquatable<MinDateKeyword>
	{
		internal const string Name = "minDate";

		public DateTime Date { get; }

		public MinDateKeyword(DateTime date)
		{
			Date = date;
		}

		public void Evaluate(EvaluationContext context)
		{
			var dateString = context.LocalInstance!.GetValue<string>();
			var date = DateTime.Parse(dateString);

			if (date < Date)
				context.LocalResult.Fail(Name, "[[provided:O]] must be on or after [[value:O]]",
					("provided", date),
					("value", Date));
		}

		public IEnumerable<Requirement> GetRequirements(JsonPointer subschemaPath, Uri baseUri, JsonPointer instanceLocation)
		{
			throw new NotImplementedException();
		}

		public bool Equals(MinDateKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Date.Equals(other.Date);
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as MinDateKeyword);
		}

		public override int GetHashCode()
		{
			return Date.GetHashCode();
		}
	}

	private class MinDateJsonConverter : JsonConverter<MinDateKeyword>
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
			writer.WritePropertyName(MinDateKeyword.Name);
			writer.WriteStringValue(value.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK"));
		}
	}

	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft7 | Draft.Draft201909 | Draft.Draft202012)]
	[JsonConverter(typeof(NonVocabMinDateJsonConverter))]
	public class NonVocabMinDateKeyword : IJsonSchemaKeyword, IEquatable<NonVocabMinDateKeyword>
	{
		internal const string Name = "minDate-nv";

		public DateTime Date { get; }

		public NonVocabMinDateKeyword(DateTime date)
		{
			Date = date;
		}

		public void Evaluate(EvaluationContext context)
		{
			var dateString = context.LocalInstance!.GetValue<string>();
			var date = DateTime.Parse(dateString);

			if (date < Date)
				context.LocalResult.Fail(Name, "[[provided:O]] must be on or after [[value:O]]",
					("provided", date),
					("value", Date));
		}

		public IEnumerable<Requirement> GetRequirements(JsonPointer subschemaPath, Uri baseUri, JsonPointer instanceLocation)
		{
			throw new NotImplementedException();
		}

		public bool Equals(NonVocabMinDateKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Date.Equals(other.Date);
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as NonVocabMinDateKeyword);
		}

		public override int GetHashCode()
		{
			return Date.GetHashCode();
		}
	}

	private class NonVocabMinDateJsonConverter : JsonConverter<NonVocabMinDateKeyword>
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
			writer.WritePropertyName(NonVocabMinDateKeyword.Name);
			writer.WriteStringValue(value.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK"));
		}
	}

	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909 | Draft.Draft202012)]
	[JsonConverter(typeof(MaxDateJsonConverter))]
	[Vocabulary("http://mydates.com/vocabulary")]
	public class MaxDateKeyword : IJsonSchemaKeyword, IEquatable<MaxDateKeyword>
	{
		internal const string Name = "maxDate";

		public DateTime Date { get; }

		public MaxDateKeyword(DateTime date)
		{
			Date = date;
		}

		public void Evaluate(EvaluationContext context)
		{
			var dateString = context.LocalInstance!.GetValue<string>();
			var date = DateTime.Parse(dateString);

			if (date > Date)
				context.LocalResult.Fail(Name, "[[provided:O]] must be on or before [[value:O]]",
					("provided", date),
					("value", Date));
		}

		public IEnumerable<Requirement> GetRequirements(JsonPointer subschemaPath, Uri baseUri, JsonPointer instanceLocation)
		{
			throw new NotImplementedException();
		}

		public bool Equals(MaxDateKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Date.Equals(other.Date);
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as MaxDateKeyword);
		}

		public override int GetHashCode()
		{
			return Date.GetHashCode();
		}
	}

	public class MaxDateJsonConverter : JsonConverter<MaxDateKeyword>
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
			writer.WritePropertyName(MaxDateKeyword.Name);
			writer.WriteStringValue(value.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK"));
		}
	}

	private static readonly JsonSerializerOptions _serializerOptions = new()
	{
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
		SchemaKeywordRegistry.Register<MinDateKeyword>();
		SchemaKeywordRegistry.Register<NonVocabMinDateKeyword>();
		SchemaKeywordRegistry.Register<MaxDateKeyword>();
	}

	[OneTimeTearDown]
	public void TearDown()
	{
		SchemaKeywordRegistry.Unregister<MinDateKeyword>();
		SchemaKeywordRegistry.Register<NonVocabMinDateKeyword>();
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
			ValidateAgainstMetaSchema = true
		};
		options.SchemaRegistry.Register(new Uri("http://mydates.com/schema"), DatesMetaSchema);
		var results = schema.Evaluate(instance, options);

		Console.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
		Console.WriteLine();
		Console.WriteLine(instance);
		Console.WriteLine();

		results.AssertInvalid();
	}

	[Test]
	public void SchemaValidation_ValidateMetaSchemaFalse_VocabularyNotKnown()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema("http://mydates.com/schema")
			.MinDate(DateTime.Now.ToUniversalTime().AddDays(-1));
		var instance = JsonNode.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"");

		var options = new EvaluationOptions();
		options.SchemaRegistry.Register(new Uri("http://mydates.com/schema"), DatesMetaSchema);
		var results = schema.Evaluate(instance, options);

		Console.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
		Console.WriteLine();
		Console.WriteLine(instance);
		Console.WriteLine();

		results.AssertValid();
	}

	[Test]
	public void SchemaValidation_ValidateMetaSchemaFalse_NonVocab_Draft201909_NoCustomKeywords()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft201909Id)
			.NonVocabMinDate(DateTime.Now.ToUniversalTime().AddDays(1));
		var instance = JsonNode.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"");

		var results = schema.Validate(instance);

		Console.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
		Console.WriteLine();
		Console.WriteLine(instance);
		Console.WriteLine();

		results.AssertValid();
	}

	[Test]
	public void SchemaValidation_ValidateMetaSchemaFalse_NonVocab_Draft201909_WithCustomKeywords()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft201909Id)
			.NonVocabMinDate(DateTime.Now.ToUniversalTime().AddDays(1));
		var instance = JsonNode.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"");

		var results = schema.Validate(instance, new EvaluationOptions { ProcessCustomKeywords = true });

		Console.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
		Console.WriteLine();
		Console.WriteLine(instance);
		Console.WriteLine();

		results.AssertInvalid();
	}

	[Test]
	public void SchemaValidation_ValidateMetaSchemaFalse_NonVocab_Draft7()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema(MetaSchemas.Draft7Id)
			.NonVocabMinDate(DateTime.Now.ToUniversalTime().AddDays(1));
		var instance = JsonNode.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"");

		var results = schema.Validate(instance);

		Console.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
		Console.WriteLine();
		Console.WriteLine(instance);
		Console.WriteLine();

		results.AssertInvalid();
	}

	[Test]
	public void SchemaValidation_ValidateMetaSchemaTrue_VocabularyKnown()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema("http://mydates.com/schema")
			.MinDate(DateTime.Now.ToUniversalTime().AddDays(-1));
		var instance = JsonNode.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"");

		var options = new EvaluationOptions
		{
			ValidateAgainstMetaSchema = true
		};
		options.SchemaRegistry.Register(new Uri("http://mydates.com/schema"), DatesMetaSchema);
		options.VocabularyRegistry.Register(DatesVocabulary);
		var results = schema.Evaluate(instance, options);

		Console.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
		Console.WriteLine();
		Console.WriteLine(instance);
		Console.WriteLine();

		results.AssertValid();
	}

	[Test]
	public void MetaSchemaValidation_VocabularyNotKnown()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema("http://mydates.com/schema")
			.MinDate(DateTime.Now.AddDays(-1));

		var schemaAsJson = JsonNode.Parse(JsonSerializer.Serialize(schema));
		var results = DatesMetaSchema.Evaluate(schemaAsJson);

		Console.WriteLine(schemaAsJson);
		Console.WriteLine();

		results.AssertInvalid();
	}

	[Test]
	public void MetaSchemaValidation_VocabularyKnown()
	{
		JsonSchema schema = new JsonSchemaBuilder()
			.Schema("http://mydates.com/schema")
			.MinDate(DateTime.Now.AddDays(-1));

		var schemaAsJson = JsonNode.Parse(JsonSerializer.Serialize(schema));
		var options = new EvaluationOptions();
		options.VocabularyRegistry.Register(DatesVocabulary);
		var results = DatesMetaSchema.Evaluate(schemaAsJson, options);

		Console.WriteLine(schemaAsJson);
		Console.WriteLine();

		results.AssertValid();
	}
}