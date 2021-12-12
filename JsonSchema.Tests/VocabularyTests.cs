using System;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;

namespace Json.Schema.Tests
{
	public class VocabularyTests
	{
		[SchemaKeyword(Name)]
		[SchemaDraft(Draft.Draft201909 | Draft.Draft202012)]
		[JsonConverter(typeof(MinDateJsonConverter))]
		public class MinDateKeyword : IJsonSchemaKeyword, IEquatable<MinDateKeyword>
		{
			internal const string Name = "minDate";

			public DateTime Date { get; }

			public MinDateKeyword(DateTime date)
			{
				Date = date;
			}

			public void Validate(ValidationContext context)
			{
				var dateString = context.LocalInstance.GetString();
				var date = DateTime.Parse(dateString);

				if (date >= Date)
					context.LocalResult.Pass();
				else
					context.LocalResult.Fail($"{date:O} must be on or after {Date:O}");
			}

			public bool Equals(MinDateKeyword other)
			{
				if (ReferenceEquals(null, other)) return false;
				if (ReferenceEquals(this, other)) return true;
				return Date.Equals(other.Date);
			}

			public override bool Equals(object obj)
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
				var date = DateTime.Parse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
				return new MinDateKeyword(date);
			}

			public override void Write(Utf8JsonWriter writer, MinDateKeyword value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(MinDateKeyword.Name);
				writer.WriteStringValue(value.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK"));
			}
		}

		[SchemaKeyword(Name)]
		[SchemaDraft(Draft.Draft201909 | Draft.Draft202012)]
		[JsonConverter(typeof(MaxDateJsonConverter))]
		public class MaxDateKeyword : IJsonSchemaKeyword, IEquatable<MaxDateKeyword>
		{
			internal const string Name = "maxDate";

			public DateTime Date { get; }

			public MaxDateKeyword(DateTime date)
			{
				Date = date;
			}

			public void Validate(ValidationContext context)
			{
				var dateString = context.LocalInstance.GetString();
				var date = DateTime.Parse(dateString);

				if (date <= Date)
					context.LocalResult.Pass();
				else
					context.LocalResult.Fail($"{date:O} must be on or before {Date:O}");
			}

			public bool Equals(MaxDateKeyword other)
			{
				if (ReferenceEquals(null, other)) return false;
				if (ReferenceEquals(this, other)) return true;
				return Date.Equals(other.Date);
			}

			public override bool Equals(object obj)
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
				var date = DateTime.Parse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
				return new MaxDateKeyword(date);
			}

			public override void Write(Utf8JsonWriter writer, MaxDateKeyword value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(MaxDateKeyword.Name);
				writer.WriteStringValue(value.Date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ssK"));
			}
		}

		private static readonly JsonSerializerOptions _serializerOptions = new JsonSerializerOptions
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

		public static readonly Vocabulary DatesVocabulary =
			new Vocabulary("http://mydates.com/vocabulary", typeof(MinDateKeyword), typeof(MaxDateKeyword));

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
			SchemaKeywordRegistry.Register<MaxDateKeyword>();
		}

		[OneTimeTearDown]
		public void TearDown()
		{
			SchemaKeywordRegistry.Unregister<MinDateKeyword>();
			SchemaKeywordRegistry.Unregister<MaxDateKeyword>();
		}

		[Test]
		public void SchemaValidation_ValidateMetaSchemaTrue_VocabularyNotKnown()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Schema("http://mydates.com/schema")
				.MinDate(DateTime.Now.AddDays(-1));
			var instance = JsonDocument.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"").RootElement;

			var options = new ValidationOptions
			{
				ValidateMetaSchema = true
			};
			options.SchemaRegistry.Register(new Uri("http://mydates.com/schema"), DatesMetaSchema);
			var results = schema.Validate(instance, options);

			Console.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
			Console.WriteLine();
			Console.WriteLine(instance);
			Console.WriteLine();
			Console.WriteLine(JsonSerializer.Serialize(results, _serializerOptions));

			Assert.IsFalse(results.IsValid);
		}

		[Test]
		public void SchemaValidation_ValidateMetaSchemaFalse_VocabularyNotKnown()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Schema("http://mydates.com/schema")
				.MinDate(DateTime.Now.AddDays(-1));
			var instance = JsonDocument.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"").RootElement;

			var options = new ValidationOptions();
			options.SchemaRegistry.Register(new Uri("http://mydates.com/schema"), DatesMetaSchema);
			var results = schema.Validate(instance, options);

			Console.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
			Console.WriteLine();
			Console.WriteLine(instance);
			Console.WriteLine();
			Console.WriteLine(JsonSerializer.Serialize(results, _serializerOptions));

			Assert.IsTrue(results.IsValid);
		}

		[Test]
		public void SchemaValidation_ValidateMetaSchemaTrue_VocabularyKnown()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Schema("http://mydates.com/schema")
				.MinDate(DateTime.Now.AddDays(-1));
			var instance = JsonDocument.Parse($"\"{DateTime.Now.ToUniversalTime():O}\"").RootElement;

			var options = new ValidationOptions
			{
				ValidateMetaSchema = true
			};
			options.SchemaRegistry.Register(new Uri("http://mydates.com/schema"), DatesMetaSchema);
			options.VocabularyRegistry.Register(DatesVocabulary);
			var results = schema.Validate(instance, options);

			Console.WriteLine(JsonSerializer.Serialize(schema, _serializerOptions));
			Console.WriteLine();
			Console.WriteLine(instance);
			Console.WriteLine();
			Console.WriteLine(JsonSerializer.Serialize(results, _serializerOptions));

			Assert.IsTrue(results.IsValid);
		}

		[Test]
		public void MetaSchemaValidation_VocabularyNotKnown()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Schema("http://mydates.com/schema")
				.MinDate(DateTime.Now.AddDays(-1));

			var schemaAsJson = JsonDocument.Parse(JsonSerializer.Serialize(schema)).RootElement;
			var results = DatesMetaSchema.Validate(schemaAsJson);

			Console.WriteLine(schemaAsJson);
			Console.WriteLine();
			Console.WriteLine(JsonSerializer.Serialize(results, _serializerOptions));

			Assert.IsFalse(results.IsValid);
		}

		[Test]
		public void MetaSchemaValidation_VocabularyKnown()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Schema("http://mydates.com/schema")
				.MinDate(DateTime.Now.AddDays(-1));

			var schemaAsJson = JsonDocument.Parse(JsonSerializer.Serialize(schema)).RootElement;
			var options = new ValidationOptions();
			options.VocabularyRegistry.Register(DatesVocabulary);
			var results = DatesMetaSchema.Validate(schemaAsJson, options);

			Console.WriteLine(schemaAsJson);
			Console.WriteLine();
			Console.WriteLine(JsonSerializer.Serialize(results, _serializerOptions));

			Assert.IsTrue(results.IsValid);
		}
	}
}
