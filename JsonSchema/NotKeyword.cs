using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaPriority(20)]
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(NotKeywordJsonConverter))]
	public class NotKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "not";

		public JsonSchema Schema { get; }

		static NotKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}
		public NotKeyword(JsonSchema value)
		{
			Schema = value;
		}

		public void Validate(ValidationContext context)
		{
			var subContext = ValidationContext.From(context,
				subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create(Name)));
			Schema.ValidateSubschema(subContext);
			context.NestedContexts.Add(subContext);
			context.IsValid = !subContext.IsValid;
		}

		private static void ConsolidateAnnotations(IEnumerable<ValidationContext> sourceContexts, ValidationContext destContext)
		{
			if (sourceContexts.Select(c => c.TryGetAnnotation(Name)).OfType<bool>().Any())
				destContext.Annotations[Name] = true;
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

	public class NotKeywordJsonConverter : JsonConverter<NotKeyword>
	{
		public override NotKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new NotKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, NotKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(NotKeyword.Name);
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}