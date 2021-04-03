using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Handles `additionalProperties`.
	/// </summary>
	[Applicator]
	[SchemaPriority(10)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[Vocabulary(Vocabularies.Applicator202012Id)]
	[JsonConverter(typeof(AdditionalPropertiesKeywordJsonConverter))]
	public class AdditionalPropertiesKeyword : IJsonSchemaKeyword, IRefResolvable, ISchemaContainer, IEquatable<AdditionalPropertiesKeyword>
	{
		internal const string Name = "additionalProperties";

		/// <summary>
		/// The schema by which to validation additional properties.
		/// </summary>
		public JsonSchema Schema { get; }

		static AdditionalPropertiesKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}
		/// <summary>
		/// Creates a new <see cref="AdditionalPropertiesKeyword"/>.
		/// </summary>
		/// <param name="value">The keyword's schema.</param>
		public AdditionalPropertiesKeyword(JsonSchema value)
		{
			Schema = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.Options.Log.EnterKeyword(Name);
			if (context.LocalInstance.ValueKind != JsonValueKind.Object)
			{
				context.Options.Log.WrongValueKind(context.LocalInstance.ValueKind);
				context.IsValid = true;
				return;
			}

			var overallResult = true;
			var annotation = (context.TryGetAnnotation(PropertiesKeyword.Name) as List<string>)?.ToList();
			List<string> evaluatedProperties;
			if (annotation == null)
			{
				context.Options.Log.Write(() => $"No annotation from {PropertiesKeyword.Name}.");
				evaluatedProperties = new List<string>();
			}
			else
			{
				context.Options.Log.Write(() => $"Annotation from {PropertiesKeyword.Name}: [{string.Join(",", annotation.Select(x => $"'{x}'"))}]");
				evaluatedProperties = annotation;
			}
			annotation = (context.TryGetAnnotation(PatternPropertiesKeyword.Name) as List<string>)?.ToList();
			if (annotation == null)
				context.Options.Log.Write(() => $"No annotation from {PatternPropertiesKeyword.Name}.");
			else
			{
				context.Options.Log.Write(() => $"Annotation from {PatternPropertiesKeyword.Name}: [{string.Join(",", annotation.Select(x => $"'{x}'"))}]");
				evaluatedProperties.AddRange(annotation);
			}
			var additionalProperties = context.LocalInstance.EnumerateObject().Where(p => !evaluatedProperties.Contains(p.Name)).ToList();
			evaluatedProperties.Clear();
			foreach (var property in additionalProperties)
			{
				if (!context.LocalInstance.TryGetProperty(property.Name, out var item))
				{
					context.Options.Log.Write(() => $"Property '{property.Name}' does not exist. Skipping.");
					continue;
				}

				context.Options.Log.Write(() => $"Validating property '{property.Name}'.");
				var subContext = ValidationContext.From(context,
					context.InstanceLocation.Combine(PointerSegment.Create($"{property.Name}")),
					item);
				Schema.ValidateSubschema(subContext);
				overallResult &= subContext.IsValid;
				context.Options.Log.Write(() => $"Property '{property.Name}' {subContext.IsValid.Validity()}.");
				if (subContext.IsValid)
					evaluatedProperties.Add(property.Name);
				else if (context.ApplyOptimizations) break;
				context.NestedContexts.Add(subContext);
			}

			if (overallResult)
			{
				if (context.TryGetAnnotation(Name) is List<string> list)
					list.AddRange(evaluatedProperties);
				else
					context.SetAnnotation(Name, evaluatedProperties);
			}
			context.IsValid = overallResult;
			context.Options.Log.ExitKeyword(Name, context.IsValid);
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
		public bool Equals(AdditionalPropertiesKeyword? other)
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
			return Equals(obj as AdditionalPropertiesKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Schema.GetHashCode();
		}
	}

	internal class AdditionalPropertiesKeywordJsonConverter : JsonConverter<AdditionalPropertiesKeyword>
	{
		public override AdditionalPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new AdditionalPropertiesKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, AdditionalPropertiesKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(AdditionalPropertiesKeyword.Name);
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}