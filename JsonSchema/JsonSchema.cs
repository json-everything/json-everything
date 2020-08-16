using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Json.Pointer;

namespace Json.Schema
{
	[JsonConverter(typeof(SchemaJsonConverter))]
	public class JsonSchema : IRefResolvable
	{
		public static readonly JsonSchema Empty = new JsonSchema(Enumerable.Empty<IJsonSchemaKeyword>(), null);
		public static readonly JsonSchema True = new JsonSchema(true);
		public static readonly JsonSchema False = new JsonSchema(false);

		public IReadOnlyCollection<IJsonSchemaKeyword> Keywords { get; }
		public IReadOnlyDictionary<string, JsonElement> OtherData { get; }

		internal bool? BoolValue { get; }

		private JsonSchema(bool value)
		{
			BoolValue = value;
		}
		internal JsonSchema(IEnumerable<IJsonSchemaKeyword> keywords, IReadOnlyDictionary<string, JsonElement> otherData)
		{
			Keywords = keywords.ToArray();
			OtherData = otherData;
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
					LocalInstance = root,
					InstanceLocation = JsonPointer.Empty,
					InstanceRoot = root,
					SchemaLocation = JsonPointer.Empty,
					SchemaRoot = this
				};

			RegisterSubschemas(context.Registry);
			ValidateSubschema(context);

			return new ValidationResults(context);
		}

		public void RegisterSubschemas(SchemaRegistry registry)
		{
			throw new NotImplementedException();
		}

		public void ValidateSubschema(ValidationContext context)
		{
			if (BoolValue.HasValue)
			{
				context.IsValid = BoolValue.Value;
				if (!context.IsValid)
					context.Message = "All values fail against the false schema";
				return;
			}

			ValidationContext newContext = null;
			foreach (var keyword in Keywords.OrderBy(k => k.Priority()))
			{
				var previousContext = newContext;
				newContext = ValidationContext.From(context,
					subschemaLocation: context.InstanceLocation.Combine(PointerSegment.Create(keyword.Keyword())));
				newContext.ParentContext = context;
				newContext.LocalSchema = this;
				newContext.ImportAnnotations(previousContext);
				if (context.HasNestedContexts)
					newContext.SiblingContexts.AddRange(context.NestedContexts);
				keyword.Validate(newContext);
				context.NestedContexts.Add(newContext);
			}

			context.ImportAnnotations(newContext);
			context.IsValid = context.NestedContexts.All(c => c.IsValid);
		}

		public JsonSchema FindSubschema(JsonPointer pointer)
		{
			IRefResolvable resolvable = this;
			foreach (var segment in pointer.Segments)
			{
				var newResolvable = resolvable.ResolvePointerSegment(segment.Value);
				if (newResolvable == null)
				{
					// TODO: Check other data
					return null;
				}

				resolvable = newResolvable;
			}

			if (!(resolvable is JsonSchema))
				resolvable = resolvable.ResolvePointerSegment(null);

			return resolvable as JsonSchema;
		}

		IRefResolvable IRefResolvable.ResolvePointerSegment(string value)
		{
			var keyword = Keywords.FirstOrDefault(k => k.Name() == value);
			return keyword as IRefResolvable;
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
			var otherData = new Dictionary<string, JsonElement>();

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
							var element = JsonDocument.ParseValue(ref reader).RootElement;
							otherData[keyword] = element.Clone();
							break;
						}

						IJsonSchemaKeyword implementation;
						if (reader.TokenType == JsonTokenType.Null)
						{
							implementation = SchemaKeywordRegistry.GetNullValuedKeyword(keywordType);
							if (implementation == null)
								throw new InvalidOperationException($"No null instance registered for keyword `{keyword}`");
						}
						else
						{
							implementation = (IJsonSchemaKeyword)JsonSerializer.Deserialize(ref reader, keywordType, options);
							if (implementation == null)
								throw new InvalidOperationException($"Could not deserialize expected keyword `{keyword}`");
						}
						keywords.Add(implementation);
						break;
					case JsonTokenType.EndObject:
						return new JsonSchema(keywords, otherData);
					default:
						throw new JsonException("Expected keyword or end of schema object");
				}
			} while (reader.Read());

			throw new JsonException("Expected token");
		}

		public override void Write(Utf8JsonWriter writer, JsonSchema value, JsonSerializerOptions options)
		{
			if (value.BoolValue == true)
			{
				writer.WriteBooleanValue(true);
				return;
			}

			if (value.BoolValue == false)
			{
				writer.WriteBooleanValue(false);
				return;
			}

			writer.WriteStartObject();
			foreach (var keyword in value.Keywords)
			{
				JsonSerializer.Serialize(writer, keyword, keyword.GetType(), options);
			}
			foreach (var data in value.OtherData)
			{
				writer.WritePropertyName(data.Key);
				JsonSerializer.Serialize(writer, data.Value, options);
			}
			writer.WriteEndObject();
		}
	}

	public static class JsonSchemaKeywordExtensions
	{
		private static readonly Dictionary<Type, string> _attributes =
			typeof(IJsonSchemaKeyword).Assembly
				.GetTypes()
				.Where(t => typeof(IJsonSchemaKeyword).IsAssignableFrom(t) &&
				            !t.IsAbstract &&
				            !t.IsInterface)
				.ToDictionary(t => t, t => t.GetCustomAttribute<SchemaKeywordAttribute>().Name);

		public static string Name(this IJsonSchemaKeyword keyword)
		{
			var keywordType = keyword.GetType();
			if (!_attributes.TryGetValue(keywordType, out var name))
			{
				name = keywordType.GetCustomAttribute<SchemaKeywordAttribute>()?.Name;
				if (name == null)
					throw new InvalidOperationException($"Type {keywordType.Name} must be decorated with {nameof(SchemaKeywordAttribute)}");

				_attributes[keywordType] = name;
			}

			return name;
		}
	}
}
