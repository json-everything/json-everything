using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Json.Schema
{
	/// <summary>
	/// Handles `requires`.
	/// </summary>
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[JsonConverter(typeof(RequiredKeywordJsonConverter))]
	public class RequiredKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "required";

		/// <summary>
		/// The required properties.
		/// </summary>
		public IReadOnlyList<string> Properties { get; }

		/// <summary>
		/// Creates a new <see cref="RequiredKeyword"/>.
		/// </summary>
		/// <param name="values">The required properties.</param>
		public RequiredKeyword(params string[] values)
		{
			Properties = values.ToList();
		}

		/// <summary>
		/// Creates a new <see cref="RequiredKeyword"/>.
		/// </summary>
		/// <param name="values">The required properties.</param>
		public RequiredKeyword(IEnumerable<string> values)
		{
			Properties = values as List<string> ?? values.ToList();
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.Object)
			{
				context.IsValid = true;
				return;
			}

			var notFound = new List<string>();
			for (int i = 0; i < Properties.Count; i++)
			{
				var property = Properties[i];
				if (!context.LocalInstance.TryGetProperty(property, out _))
					notFound.Add(property);
				if (notFound.Count != 0 && context.ApplyOptimizations) break;
			}

			context.IsValid = notFound.Count == 0;
			if (!context.IsValid)
				context.Message = $"Required properties [{string.Join(", ", notFound)}] were not present";
		}
	}

	internal class RequiredKeywordJsonConverter : JsonConverter<RequiredKeyword>
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