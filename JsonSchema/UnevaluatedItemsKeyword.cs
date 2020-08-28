using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[Applicator]
	[SchemaPriority(30)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[JsonConverter(typeof(UnevaluatedItemsKeywordJsonConverter))]
	public class UnevaluatedItemsKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "unevaluatedItems";

		public JsonSchema Schema { get; }

		static UnevaluatedItemsKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}
		public UnevaluatedItemsKeyword(JsonSchema value)
		{
			Schema = value;
		}

		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.Array)
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
			annotation = context.TryGetAnnotation(Name);
			if (annotation is bool) // is only ever true
			{
				context.IsValid = true;
				return;
			}
			for (int i = startIndex; i < context.LocalInstance.GetArrayLength(); i++)
			{
				var item = context.LocalInstance[i];
				var subContext = ValidationContext.From(context,
					context.InstanceLocation.Combine(PointerSegment.Create($"{i}")),
					item);
				Schema.ValidateSubschema(subContext);
				overallResult &= subContext.IsValid;
				if (!overallResult && context.ApplyOptimizations) break;
				context.NestedContexts.Add(subContext);
			}

			if (overallResult)
				context.SetAnnotation(Name, true);
			context.IsValid = overallResult;
		}

		private static void ConsolidateAnnotations(IEnumerable<ValidationContext> sourceContexts, ValidationContext destContext)
		{
			if (sourceContexts.Select(c => c.TryGetAnnotation(Name)).OfType<bool>().Any())
				destContext.SetAnnotation(Name, true);
		}

		public IRefResolvable ResolvePointerSegment(string value)
		{
			return value == null ? Schema : null;
		}

		public void RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			Schema.RegisterSubschemas(registry, currentUri);
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
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}