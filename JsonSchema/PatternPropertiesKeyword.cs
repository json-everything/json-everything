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
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[Vocabulary(Vocabularies.Applicator202012Id)]
	[JsonConverter(typeof(PatternPropertiesKeywordJsonConverter))]
	public class PatternPropertiesKeyword : IJsonSchemaKeyword, IRefResolvable, IKeyedSchemaCollector, IEquatable<PatternPropertiesKeyword>
	{
		internal const string Name = "patternProperties";

		/// <summary>
		/// The pattern-keyed schemas.
		/// </summary>
		public IReadOnlyDictionary<Regex, JsonSchema> Patterns { get; }
		/// <summary>
		/// If any pattern is invalid or unsupported by <see cref="Regex"/>, it will appear here.
		/// </summary>
		/// <remarks>
		/// All validations will fail if this is populated.
		/// </remarks>
		public IReadOnlyList<string>? InvalidPatterns { get; }

		IReadOnlyDictionary<string, JsonSchema> IKeyedSchemaCollector.Schemas => Patterns.ToDictionary(x => x.Key.ToString(), x => x.Value);

		static PatternPropertiesKeyword()
		{
			ValidationResults.RegisterConsolidationMethod(ConsolidateAnnotations);
		}

		/// <summary>
		/// Creates a new <see cref="PatternPropertiesKeyword"/>.
		/// </summary>
		/// <param name="values">The pattern-keyed schemas.</param>
		public PatternPropertiesKeyword(IReadOnlyDictionary<Regex, JsonSchema> values)
		{
			Patterns = values ?? throw new ArgumentNullException(nameof(values));
		}
		internal PatternPropertiesKeyword(IReadOnlyDictionary<Regex, JsonSchema> values, IReadOnlyList<string> invalidPatterns)
		{
			Patterns = values ?? throw new ArgumentNullException(nameof(values));
			InvalidPatterns = invalidPatterns;
		}

		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		public void Validate(ValidationContext context)
		{
			context.EnterKeyword(Name);
			
			if (context.LocalInstance.ValueKind != JsonValueKind.Object)
			{
				context.LocalResult.Pass();
				context.WrongValueKind(context.LocalInstance.ValueKind);
				return;
			}

			context.Options.LogIndentLevel++;
			var overallResult = true;
			var evaluatedProperties = new List<string>();
			var instanceProperties = context.LocalInstance.EnumerateObject().ToList();
			foreach (var entry in Patterns)
			{
				var schema = entry.Value;
				var pattern = entry.Key;
				foreach (var instanceProperty in instanceProperties.Where(p => pattern.IsMatch(p.Name)))
				{
					context.Log(() => $"Validating property '{instanceProperty.Name}'.");
					context.Push(context.InstanceLocation.Combine(PointerSegment.Create($"{instanceProperty.Name}")),
						instanceProperty.Value,
						context.SchemaLocation.Combine(PointerSegment.Create($"{pattern}")));
					schema.ValidateSubschema(context);
					overallResult &= context.LocalResult.IsValid;
					context.Log(() => $"Property '{instanceProperty.Name}' {context.LocalResult.IsValid.GetValidityString()}.");
					context.Pop();
					if (!overallResult && context.ApplyOptimizations) break;
					evaluatedProperties.Add(instanceProperty.Name);
				}
			}
			if (InvalidPatterns?.Any() ?? false)
			{
				foreach (var pattern in InvalidPatterns)
				{
					context.Push(subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create($"{pattern}")));
					context.LocalResult.Fail($"The regular expression `{pattern}` is either invalid or not supported");
					overallResult = false;
					context.Log(() => $"Discovered invalid pattern '{pattern}'.");
					context.Pop();
					if (!overallResult && context.ApplyOptimizations) break;
				}
			}
			context.Options.LogIndentLevel--;

			if (context.LocalResult.TryGetAnnotation(Name) is List<string> annotation)
				annotation.AddRange(evaluatedProperties);
			else
				context.LocalResult.SetAnnotation(Name, evaluatedProperties);
			if (overallResult)
				context.LocalResult.Pass();
			else
				context.LocalResult.Fail();
			context.ExitKeyword(Name, context.LocalResult.IsValid);
		}

		IRefResolvable? IRefResolvable.ResolvePointerSegment(string? value)
		{
			throw new NotImplementedException();
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			foreach (var schema in Patterns.Values)
			{
				schema.RegisterSubschemas(registry, currentUri);
			}
		}

		private static void ConsolidateAnnotations(ValidationResults localResults)
		{
			var allProperties = localResults.NestedResults.Select(c => c.TryGetAnnotation(Name))
				.Where(a => a != null)
				.Cast<List<string>>()
				.SelectMany(a => a)
				.Distinct()
				.ToList();
			if (localResults.TryGetAnnotation(Name) is List<string> annotation)
				annotation.AddRange(allProperties);
			else if (allProperties.Any())
				localResults.SetAnnotation(Name, allProperties);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(PatternPropertiesKeyword? other)
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
			return ((IKeyedSchemaCollector) this).Schemas.GetStringDictionaryHashCode();
		}
	}

	internal class PatternPropertiesKeywordJsonConverter : JsonConverter<PatternPropertiesKeyword>
	{
		public override PatternPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var patternProps = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options);
			var schemas = new Dictionary<Regex, JsonSchema>();
			var invalidProps = new List<string>();
			foreach (var prop in patternProps)
			{
				try
				{
					var regex = new Regex(prop.Key, RegexOptions.ECMAScript | RegexOptions.Compiled);	
					schemas.Add(regex, prop.Value);
				}
				catch
				{
					invalidProps.Add(prop.Key);
				}
			}
			return new PatternPropertiesKeyword(schemas, invalidProps);
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