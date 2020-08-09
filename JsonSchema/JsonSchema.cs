using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[JsonConverter(typeof(SchemaJsonConverter))]
	public class JsonSchema
	{
		private static Type[] _firstKeywords = {typeof(IdKeyword)};
		private static Type[] _lastKeywords = { };

		private bool? _boolValue;

		public static readonly JsonSchema Empty = new JsonSchema(Enumerable.Empty<IJsonSchemaKeyword>());
		public static readonly JsonSchema True = new JsonSchema(Enumerable.Empty<IJsonSchemaKeyword>()) {_boolValue = true};
		public static readonly JsonSchema False = new JsonSchema(Enumerable.Empty<IJsonSchemaKeyword>()) { _boolValue = false};

		public IReadOnlyCollection<IJsonSchemaKeyword> Keywords { get; }

		internal JsonSchema(IEnumerable<IJsonSchemaKeyword> keywords)
		{
			Keywords = keywords.ToArray();
		}

		public static JsonSchema FromFile(string fileName)
		{
			var text = File.ReadAllText(fileName);
			return FromText(text);
		}

		public static JsonSchema FromText(string jsonText)
		{
			return JsonSerializer.Deserialize<JsonSchema>(jsonText);
		}

		public static JsonSchema FromStream(StreamReader reader)
		{
			throw new NotImplementedException();
			//return JsonSerializer.Deserialize<JsonSchema>()
		}

		public ValidationResults Validate(JsonElement root)
		{
			var context = new ValidationContext
				{
					Registry = new SchemaRegistry(),
					Instance = root,
					InstanceLocation = JsonPointer.Empty,
					InstanceRoot = root,
					SchemaLocation = JsonPointer.Empty
				};

			return ValidateSubschema(context);
		}

		public ValidationResults ValidateSubschema(ValidationContext context)
		{
			if (_boolValue.HasValue)
			{
				return _boolValue.Value
					? ValidationResults.Success()
					: ValidationResults.Fail("All values fail against the false schema");
			}

			var subschemaResults = new List<ValidationResults>();

			foreach (var keyword in Keywords.OrderBy(k => k.Priority()))
			{
				var newContext = ValidationContext.From(context, subschemaLocation: context.InstanceLocation.Combine(PointerSegment.Create(keyword.Keyword())));
				subschemaResults.Add(keyword.Validate(newContext));
			}

			var failures = subschemaResults.Where(r => !r.IsValid).ToArray();
			if (failures.Any())
				return ValidationResults.Fail(failures);

			return ValidationResults.Success(subschemaResults);
		}
	}

	public class SchemaJsonConverter : JsonConverter<JsonSchema>
	{
		public override JsonSchema Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.True) return JsonSchema.True;
			if (reader.TokenType == JsonTokenType.False) return JsonSchema.False;

			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("JSON Schema must be true, false, or an object");

			if (!reader.Read())
				throw new JsonException("Expected token");

			var keywords = new List<IJsonSchemaKeyword>();

			do
			{
				switch (reader.TokenType)
				{
					case JsonTokenType.Comment:
						break;
					case JsonTokenType.PropertyName:
						var keyword = reader.GetString();
						reader.Read();
						var keywordType = SchemaKeywordRegistry.GetImplementationType(keyword);
						if (keywordType == null)
						{
							JsonDocument.ParseValue(ref reader);
							break;
						}
						var implementation = (IJsonSchemaKeyword)JsonSerializer.Deserialize(ref reader, keywordType, options);
						keywords.Add(implementation);
						break;
					case JsonTokenType.EndObject:
						return new JsonSchema(keywords);
					default:
						throw new JsonException("Expected keyword or end of schema object");
				}
			} while (reader.Read());

			throw new JsonException("Expected token");
		}

		public override void Write(Utf8JsonWriter writer, JsonSchema value, JsonSerializerOptions options)
		{
			writer.WriteStartObject();
			foreach (var keyword in value.Keywords)
			{
				JsonSerializer.Serialize(writer, keyword, keyword.GetType(), options);
			}
			writer.WriteEndObject();
		}
	}
}
