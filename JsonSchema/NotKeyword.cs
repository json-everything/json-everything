using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaPriority(10)]
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(NotKeywordJsonConverter))]
	public class NotKeyword : IJsonSchemaKeyword
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

		public ValidationResults Validate(ValidationContext context)
		{
			var subContext = ValidationContext.From(context,
				subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create(Name)));
			var results = Schema.ValidateSubschema(subContext);

			var result = !results.IsValid
				? ValidationResults.Success(context)
				: ValidationResults.Fail(context);

			result.AddNestedResults(results);
			return result;
		}

		private static void ConsolidateAnnotations(IEnumerable<ValidationContext> sourceContexts, ValidationContext destContext)
		{
			if (sourceContexts.Select(c => c.TryGetAnnotation(Name)).OfType<bool>().Any())
				destContext.Annotations[Name] = true;
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