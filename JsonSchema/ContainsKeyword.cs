using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Handles `contains`.
	/// </summary>
	[Applicator]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[JsonConverter(typeof(ContainsKeywordJsonConverter))]
	public class ContainsKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "contains";

		/// <summary>
		/// The schema to match.
		/// </summary>
		public JsonSchema Schema { get; }

		/// <summary>
		/// Creates a new <see cref="ContainsKeyword"/>.
		/// </summary>
		/// <param name="value">The schema to match.</param>
		public ContainsKeyword(JsonSchema value)
		{
			Schema = value;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.Array)
			{
				context.IsValid = true;
				return;
			}

			var count = context.LocalInstance.GetArrayLength();
			for (int i = 0; i < count; i++)
			{
				var subContext = ValidationContext.From(context,
					context.InstanceLocation.Combine(PointerSegment.Create($"{i}")),
					context.LocalInstance[i]);
				Schema.ValidateSubschema(subContext);
				context.NestedContexts.Add(subContext);
			}

			var found = context.NestedContexts.Count(r => r.IsValid);
			var minContainsKeyword = context.LocalSchema.Keywords.OfType<MinContainsKeyword>().FirstOrDefault();
			if (minContainsKeyword != null && minContainsKeyword.Value == 0)
				context.IsValid = true;
			else
				context.IsValid = found != 0;
			if (context.IsValid)
				context.SetAnnotation(Name, found);
			else
				context.Message = "Expected array to contain at least one item that matched the schema, but it did not";
		}

		IRefResolvable IRefResolvable.ResolvePointerSegment(string value)
		{
			return value == null ? Schema : null;
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			Schema.RegisterSubschemas(registry, currentUri);
		}
	}

	internal class ContainsKeywordJsonConverter : JsonConverter<ContainsKeyword>
	{
		public override ContainsKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new ContainsKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, ContainsKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(ContainsKeyword.Name);
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}