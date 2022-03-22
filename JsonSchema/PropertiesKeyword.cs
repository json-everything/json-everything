using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Handles `properties`.
	/// </summary>
	[Applicator]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Applicator201909Id)]
	[Vocabulary(Vocabularies.Applicator202012Id)]
	[JsonConverter(typeof(PropertiesKeywordJsonConverter))]
	public class PropertiesKeyword : IJsonSchemaKeyword, IRefResolvable, IKeyedSchemaCollector, IEquatable<PropertiesKeyword>
	{
		internal const string Name = "properties";

		/// <summary>
		/// The property schemas.
		/// </summary>
		public IReadOnlyDictionary<string, JsonSchema> Properties { get; }

		IReadOnlyDictionary<string, JsonSchema> IKeyedSchemaCollector.Schemas => Properties;

		static PropertiesKeyword()
		{
			ValidationResults.RegisterConsolidationMethod(ConsolidateAnnotations);
		}

		/// <summary>
		/// Creates a new <see cref="PropertiesKeyword"/>.
		/// </summary>
		/// <param name="values">The property schemas.</param>
		public PropertiesKeyword(IReadOnlyDictionary<string, JsonSchema> values)
		{
			Properties = values ?? throw new ArgumentNullException(nameof(values));
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
			foreach (var property in Properties)
			{
				context.Log(() => $"Validating property '{property.Key}'.");
				var schema = property.Value;
				var name = property.Key;
				if (!context.LocalInstance.TryGetProperty(name, out var item))
				{
					context.Log(() => $"Property '{property.Key}' does not exist. Skipping.");
					continue;
				}
				
				context.Push(context.InstanceLocation.Combine(PointerSegment.Create($"{name}")),
					item,
					context.SchemaLocation.Combine(PointerSegment.Create($"{name}")));
				schema.ValidateSubschema(context);
				var localResult = context.LocalResult.IsValid;
				overallResult &= localResult;
				evaluatedProperties.Add(name);
				context.Log(() => $"Property '{property.Key}' {localResult.GetValidityString()}.");
				context.Pop();
				if (!overallResult && context.ApplyOptimizations) break;
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

		IRefResolvable? IRefResolvable.ResolvePointerSegment(string? value)
		{
			throw new NotImplementedException();
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			foreach (var schema in Properties.Values)
			{
				schema.RegisterSubschemas(registry, currentUri);
			}
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(PropertiesKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (Properties.Count != other.Properties.Count) return false;
			var byKey = Properties.Join(other.Properties,
					td => td.Key,
					od => od.Key,
					(td, od) => new { ThisDef = td.Value, OtherDef = od.Value })
				.ToList();
			if (byKey.Count != Properties.Count) return false;

			return byKey.All(g => Equals(g.ThisDef, g.OtherDef));
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as PropertiesKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Properties.GetStringDictionaryHashCode();
		}
	}

	internal class PropertiesKeywordJsonConverter : JsonConverter<PropertiesKeyword>
	{
		public override PropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var schema = JsonSerializer.Deserialize<Dictionary<string, JsonSchema>>(ref reader, options);
			return new PropertiesKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, PropertiesKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(PropertiesKeyword.Name);
			writer.WriteStartObject();
			foreach (var kvp in value.Properties)
			{
				writer.WritePropertyName(kvp.Key);
				JsonSerializer.Serialize(writer, kvp.Value, options);
			}
			writer.WriteEndObject();
		}
	}
}