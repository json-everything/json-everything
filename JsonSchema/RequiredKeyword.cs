using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(RequiredKeywordJsonConverter))]
	public class RequiredKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "required";

		public List<string> Properties { get; }

		public RequiredKeyword(params string[] values)
		{
			Properties = values.ToList();
		}

		public RequiredKeyword(IEnumerable<string> values)
		{
			Properties = values as List<string> ?? values.ToList();
		}

		public ValidationResults Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Object)
				return null;

			var notFound = new List<string>();
			for (int i = 0; i < Properties.Count; i++)
			{
				// TODO: add shortcutting
				var property = Properties[i];
				if (!context.Instance.TryGetProperty(property, out _))
					notFound.Add(property);
			}

			if (notFound.Count == 0) return ValidationResults.Success(context);
			return ValidationResults.Fail(context, $"Required properties [{string.Join(", ", notFound)}] were not present");
		}
	}

	public class RequiredKeywordJsonConverter : JsonConverter<RequiredKeyword>
	{
		public override RequiredKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var document = JsonDocument.ParseValue(ref reader);

			if (document.RootElement.ValueKind != JsonValueKind.Array)
				throw new JsonException("Expected array");

			return new RequiredKeyword(document.RootElement.EnumerateArray()
				.Select(e => e.GetString()));
		}
		public override void Write(Utf8JsonWriter writer, RequiredKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(RequiredKeyword.Name);
			writer.WriteStartArray();
			foreach (var property in value.Properties)
			{
				writer.WriteStringValue(property);
			}
			writer.WriteEndArray();
		}
	}
}