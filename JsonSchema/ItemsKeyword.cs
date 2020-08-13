using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(ItemsKeywordJsonConverter))]
	public class ItemsKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "items";

		public JsonSchema SingleSchema { get; }
		public IReadOnlyList<JsonSchema> ArraySchemas { get; }

		static ItemsKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}
		public ItemsKeyword(JsonSchema value)
		{
			SingleSchema = value;
		}

		public ItemsKeyword(params JsonSchema[] values)
		{
			ArraySchemas = values.ToList();
		}

		public ItemsKeyword(IEnumerable<JsonSchema> values)
		{
			ArraySchemas = values.ToList();
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Array)
				return null;
			
			var subResults = new List<ValidationResults>();
			var overallResult = true;
			if (SingleSchema != null)
			{
				for (int i = 0; i < context.Instance.GetArrayLength(); i++)
				{
					var item = context.Instance[i];
					var subContext = ValidationContext.From(context,
						context.InstanceLocation.Combine(PointerSegment.Create($"{i}")),
						item);
					var results = SingleSchema.ValidateSubschema(subContext);
					overallResult &= results.IsValid;
					subResults.Add(results);
				}

				context.Annotations[Name] = true;
			}
			else // array
			{
				var maxEvaluations = Math.Min(ArraySchemas.Count, context.Instance.GetArrayLength());
				for (int i = 0; i < maxEvaluations; i++)
				{
					var schema = ArraySchemas[i];
					var item = context.Instance[i];
					var subContext = ValidationContext.From(context,
						context.InstanceLocation.Combine(PointerSegment.Create($"{i}")),
						item,
						context.SchemaLocation.Combine(PointerSegment.Create($"{i}")));
					var results = schema.ValidateSubschema(subContext);
					overallResult &= results.IsValid;
					subResults.Add(results);
				}

				context.Annotations[Name] = maxEvaluations;
			}

			var result = overallResult
				? ValidationResults.Success(context)
				: ValidationResults.Fail(context);
			result.AddNestedResults(subResults);
			return result;
		}

		private static void ConsolidateAnnotations(IEnumerable<ValidationContext> sourceContexts, ValidationContext destContext)
		{
			object value;
			var allAnnotations = sourceContexts.Select(c => c.TryGetAnnotation(Name))
				.Where(a => a != null)
				.ToList();
			if (allAnnotations.OfType<bool>().Any())
				value = true;
			else
				value = allAnnotations.OfType<int>().DefaultIfEmpty(-1).Max();
			if (!Equals(value, -1))
				destContext.Annotations[Name] = value;
		}
	}

	public class ItemsKeywordJsonConverter : JsonConverter<ItemsKeyword>
	{
		public override ItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
			{
				var schemas = JsonSerializer.Deserialize<List<JsonSchema>>(ref reader, options);
				return new ItemsKeyword(schemas);
			}
			
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);
			return new ItemsKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, ItemsKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(ItemsKeyword.Name);
			if (value.SingleSchema != null)
				JsonSerializer.Serialize(writer, value.SingleSchema, options);
			else
			{
				writer.WriteStartArray();
				foreach (var schema in value.ArraySchemas)
				{
					JsonSerializer.Serialize(writer, schema, options);
				}
				writer.WriteEndArray();
			}
		}
	}
}