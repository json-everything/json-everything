using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaPriority(30)]
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(UnevaluatedItemsKeywordJsonConverter))]
	public class UnevaluatedItemsKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "unevaluatedItems";

		public JsonSchema Value { get; }

		public UnevaluatedItemsKeyword(JsonSchema value)
		{
			Value = value;
		}

		public void Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Array)
			{
				context.IsValid = true;
				return;
			}

			var overallResult = true;
			int startIndex = 0;
			var annotation = context.TryGetAnnotation(ItemsKeyword.Name);
			if (annotation != null)
			{
				if (annotation is bool) // is only ever true or a number
				{
					context.IsValid = true;
					return;
				}
				startIndex = (int) annotation;
			}
			annotation = context.TryGetAnnotation(AdditionalItemsKeyword.Name);
			if (annotation is bool) // is only ever true
			{
				context.IsValid = true;
				return;
			}
			for (int i = startIndex; i < context.Instance.GetArrayLength(); i++)
			{
				var item = context.Instance[i];
				var subContext = ValidationContext.From(context,
					context.InstanceLocation.Combine(PointerSegment.Create($"{i}")),
					item);
				Value.ValidateSubschema(subContext);
				overallResult &= subContext.IsValid;
				context.NestedContexts.Add(subContext);
			}

			context.IsValid = overallResult;
		}
	}

	public class UnevaluatedItemsKeywordJsonConverter : JsonConverter<UnevaluatedItemsKeyword>
	{
		public override UnevaluatedItemsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new UnevaluatedItemsKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, UnevaluatedItemsKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(UnevaluatedItemsKeyword.Name);
			JsonSerializer.Serialize(writer, value.Value, options);
		}
	}
}