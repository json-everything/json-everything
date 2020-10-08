using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Handles `patternProperties`.
	/// </summary>
	[Applicator]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[JsonConverter(typeof(PatternPropertiesKeywordJsonConverter))]
	public class PatternPropertiesKeyword : IJsonSchemaKeyword, IEquatable<PatternPropertiesKeyword>
	{
		internal const string Name = "patternProperties";

		/// <summary>
		/// The pattern-keyed schemas.
		/// </summary>
		public IReadOnlyDictionary<Regex, JsonSchema> Patterns { get; }

		static PatternPropertiesKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}

		/// <summary>
		/// Creates a new <see cref="PatternPropertiesKeyword"/>.
		/// </summary>
		/// <param name="values">The pattern-keyed schemas.</param>
		public PatternPropertiesKeyword(IReadOnlyDictionary<Regex, JsonSchema> values)
		{
			Patterns = values;
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
			var evaluatedProperties = new List<string>();
			var instanceProperties = context.LocalInstance.EnumerateObject().ToList();
			foreach (var entry in Patterns)
			{
				var schema = entry.Value;
				var pattern = entry.Key;
				foreach (var instanceProperty in instanceProperties.Where(p => pattern.IsMatch(p.Name)))
				{
					var subContext = ValidationContext.From(context,
						context.InstanceLocation.Combine(PointerSegment.Create($"{instanceProperty.Name}")),
						instanceProperty.Value,
						context.SchemaLocation.Combine(PointerSegment.Create($"{pattern}")));
					schema.ValidateSubschema(subContext);
					overallResult &= subContext.IsValid;
					if (!overallResult && context.ApplyOptimizations) break;
					context.NestedContexts.Add(subContext);
					evaluatedProperties.Add(instanceProperty.Name);
				}
			}

			if (overallResult)
			{
				if (context.TryGetAnnotation(Name) is List<string> annotation)
					annotation.AddRange(evaluatedProperties);
				else
					context.SetAnnotation(Name, evaluatedProperties);
			}
			// TODO: add message
			context.IsValid = overallResult;
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

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(PatternPropertiesKeyword other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (Patterns.Count != other.Patterns.Count) return false;
			var byKey = Patterns.Join(other.Patterns,
					td => td.Key.ToString(),
					od => od.Key.ToString(),
					(td, od) => new {ThisDef = td.Value, OtherDef = od.Value})
				.ToList();
			if (byKey.Count != Patterns.Count) return false;

			return byKey.All(g => Equals(g.ThisDef, g.OtherDef));
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as PatternPropertiesKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Patterns?.GetCollectionHashCode() ?? 0;
		}
	}

	internal class PatternPropertiesKeywordJsonConverter : JsonConverter<PatternPropertiesKeyword>
	{
		public override PatternPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var schemas = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options)
				.ToDictionary(kvp => new Regex(kvp.Key, RegexOptions.ECMAScript | RegexOptions.Compiled), kvp => kvp.Value);
			return new PatternPropertiesKeyword(schemas);
		}
		public override void Write(Utf8JsonWriter writer, PatternPropertiesKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(PatternPropertiesKeyword.Name);
			writer.WriteStartObject();
			foreach (var schema in value.Patterns)
			{
				writer.WritePropertyName(schema.Key.ToString());
				JsonSerializer.Serialize(writer, schema.Value, options);
			}
			writer.WriteEndObject();
		}
	}
}