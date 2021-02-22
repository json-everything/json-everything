using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Handles `unevaluatedProperties`.
	/// </summary>
	[Applicator]
	[SchemaPriority(30)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[Vocabulary(Vocabularies.Applicator202012Id)]
	[JsonConverter(typeof(UnevaluatedPropertiesKeywordJsonConverter))]
	public class UnevaluatedPropertiesKeyword : IJsonSchemaKeyword, IRefResolvable, ISchemaContainer, IEquatable<UnevaluatedPropertiesKeyword>
	{
		internal const string Name = "unevaluatedProperties";

		/// <summary>
		/// The schema by which to validation additional properties.
		/// </summary>
		public JsonSchema Schema { get; }

		static UnevaluatedPropertiesKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}
		/// <summary>
		/// Creates a new <see cref="UnevaluatedPropertiesKeyword"/>.
		/// </summary>
		/// <param name="value"></param>
		public UnevaluatedPropertiesKeyword(JsonSchema value)
		{
			Schema = value ?? throw new ArgumentNullException(nameof(value));
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

			var overallResult = true;
			var annotation = context.TryGetAnnotation(PropertiesKeyword.Name);
			var evaluatedProperties = (annotation as List<string>)?.ToList() ?? new List<string>();
			annotation = context.TryGetAnnotation(PatternPropertiesKeyword.Name);
			evaluatedProperties.AddRange(annotation as List<string> ?? Enumerable.Empty<string>());
			annotation = context.TryGetAnnotation(AdditionalPropertiesKeyword.Name);
			evaluatedProperties.AddRange(annotation as List<string> ?? Enumerable.Empty<string>());
			annotation = context.TryGetAnnotation(Name);
			evaluatedProperties.AddRange(annotation as List<string> ?? Enumerable.Empty<string>());
			var unevaluatedProperties = context.LocalInstance.EnumerateObject().Where(p => !evaluatedProperties.Contains(p.Name)).ToList();
			evaluatedProperties.Clear();
			foreach (var property in unevaluatedProperties)
			{
				if (!context.LocalInstance.TryGetProperty(property.Name, out var item)) continue;

				var subContext = ValidationContext.From(context,
					context.InstanceLocation.Combine(PointerSegment.Create($"{property.Name}")),
					item);
				Schema.ValidateSubschema(subContext);
				overallResult &= subContext.IsValid;
				if (!overallResult && context.ApplyOptimizations) break;
				context.NestedContexts.Add(subContext);
				if (subContext.IsValid)
					evaluatedProperties.Add(property.Name);
			}

			if (overallResult)
				context.SetAnnotation(Name, evaluatedProperties);
			// TODO: add message
			context.IsValid = overallResult;
			context.ConsolidateAnnotations();
		}

		private static void ConsolidateAnnotations(IEnumerable<ValidationContext> sourceContexts, ValidationContext destContext)
		{
			var allProperties = sourceContexts.Select(c => c.TryGetAnnotation(Name))
				.Where(a => a != null)
				.Cast<List<string>>()
				.SelectMany(a => a)
				.Distinct()
				.ToList();
			if (destContext.TryGetAnnotation(Name) is List<string> annotation)
				annotation.AddRange(allProperties);
			else if (allProperties.Any())
				destContext.SetAnnotation(Name, allProperties);
		}

		IRefResolvable? IRefResolvable.ResolvePointerSegment(string? value)
		{
			return value == null ? Schema : null;
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			Schema.RegisterSubschemas(registry, currentUri);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(UnevaluatedPropertiesKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(Schema, other.Schema);
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as UnevaluatedPropertiesKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Schema.GetHashCode();
		}
	}

	internal class UnevaluatedPropertiesKeywordJsonConverter : JsonConverter<UnevaluatedPropertiesKeyword>
	{
		public override UnevaluatedPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new UnevaluatedPropertiesKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, UnevaluatedPropertiesKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(UnevaluatedPropertiesKeyword.Name);
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}