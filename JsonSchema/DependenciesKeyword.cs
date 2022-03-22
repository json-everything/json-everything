using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Handles `dependencies`.
	/// </summary>
	[SchemaPriority(10)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft6)]
	[SchemaDraft(Draft.Draft7)]
	[JsonConverter(typeof(DependenciesKeywordJsonConverter))]
	public class DependenciesKeyword : IJsonSchemaKeyword, IRefResolvable, IKeyedSchemaCollector, IEquatable<DependenciesKeyword>
	{
		internal const string Name = "dependencies";

		/// <summary>
		/// The collection of dependencies.
		/// </summary>
		public IReadOnlyDictionary<string, SchemaOrPropertyList> Requirements { get; }

		IReadOnlyDictionary<string, JsonSchema> IKeyedSchemaCollector.Schemas =>
			Requirements.Where(x => x.Value.Schema != null)
				.ToDictionary(x => x.Key, x => x.Value.Schema!);

		static DependenciesKeyword()
		{
			ValidationResults.RegisterConsolidationMethod(ConsolidateAnnotations);
		}
		/// <summary>
		/// Creates a new <see cref="DependenciesKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of dependencies.</param>
		public DependenciesKeyword(IReadOnlyDictionary<string, SchemaOrPropertyList> values)
		{
			Requirements = values;
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

			var overallResult = true;
			var evaluatedProperties = new List<string>();
			foreach (var property in Requirements)
			{
				context.Log(() => $"Validating property '{property.Key}'.");
				var requirements = property.Value;
				var name = property.Key;
				if (!context.LocalInstance.TryGetProperty(name, out _))
				{
					context.Log(() => $"Property '{property.Key}' does not exist. Skipping.");
					continue;
				}

				context.Options.LogIndentLevel++;
				if (requirements.Schema != null)
				{
					context.Log(() => "Found schema requirement.");
					context.Push(subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create($"{name}")));
					requirements.Schema.ValidateSubschema(context);
					overallResult &= context.LocalResult.IsValid;
					if (context.LocalResult.IsValid)
						evaluatedProperties.Add(name);
					context.Log(() => $"Property '{property.Key}' {context.LocalResult.IsValid.GetValidityString()}.");
					context.Pop();
				}
				else
				{
					context.Log(() => "Found property list requirement.");
					var missingDependencies = new List<string>();
					foreach (var dependency in requirements.Requirements!)
					{
						if (context.LocalInstance.TryGetProperty(dependency, out _)) continue;

						overallResult = false;
						missingDependencies.Add(dependency);
					}

					if (!missingDependencies.Any())
						evaluatedProperties.Add(name);
					else
					{
						context.Log(() => $"Missing properties [{string.Join(",", missingDependencies.Select(x => $"'{x}'"))}].");
						overallResult = false;
					}
				}
				context.Options.LogIndentLevel--;

				if (!overallResult && context.ApplyOptimizations) break;
			}

			if (overallResult)
				context.LocalResult.Pass();
			else
				context.LocalResult.Fail($"The following properties failed their dependent schemas: {JsonSerializer.Serialize(evaluatedProperties)}");
			context.ExitKeyword(Name, context.LocalResult.IsValid);
		}

		private static void ConsolidateAnnotations(ValidationResults localResults)
		{
			var allDependencies = localResults.NestedResults.Select(c => c.TryGetAnnotation(Name))
				.Where(a => a != null)
				.Cast<List<string>>()
				.SelectMany(a => a)
				.Distinct()
				.ToList();
			if (localResults.TryGetAnnotation(Name) is List<string> annotation)
				annotation.AddRange(allDependencies);
			else if (allDependencies.Any())
				localResults.SetAnnotation(Name, allDependencies);
		}

		IRefResolvable? IRefResolvable.ResolvePointerSegment(string? value)
		{
			throw new NotImplementedException();
		}

		void IRefResolvable.RegisterSubschemas(SchemaRegistry registry, Uri currentUri)
		{
			foreach (var requirement in Requirements.Values)
			{
				requirement.Schema?.RegisterSubschemas(registry, currentUri);
			}
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(DependenciesKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (Requirements.Count != other.Requirements.Count) return false;
			var byKey = Requirements.Join(other.Requirements,
					td => td.Key,
					od => od.Key,
					(td, od) => new {ThisDef = td.Value, OtherDef = od.Value})
				.ToList();
			if (byKey.Count != Requirements.Count) return false;

			return byKey.All(g => Equals(g.ThisDef, g.OtherDef));
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as DependenciesKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Requirements.GetStringDictionaryHashCode();
		}
	}

	internal class DependenciesKeywordJsonConverter : JsonConverter<DependenciesKeyword>
	{
		public override DependenciesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var dependencies = JsonSerializer.Deserialize<Dictionary<string, SchemaOrPropertyList>>(ref reader, options);
			return new DependenciesKeyword(dependencies);
		}
		public override void Write(Utf8JsonWriter writer, DependenciesKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(DependenciesKeyword.Name);
			writer.WriteStartObject();
			foreach (var kvp in value.Requirements)
			{
				writer.WritePropertyName(kvp.Key);
				JsonSerializer.Serialize(writer, kvp.Value, options);
			}
			writer.WriteEndObject();
		}
	}

	/// <summary>
	/// A holder for either a schema dependency or a requirements dependency.
	/// </summary>
	[JsonConverter(typeof(SchemaOrPropertyListJsonConverter))]
	public class SchemaOrPropertyList : IEquatable<SchemaOrPropertyList>
	{
		/// <summary>
		/// The schema dependency.
		/// </summary>
		public JsonSchema? Schema { get; }
		/// <summary>
		/// The property dependency.
		/// </summary>
		public List<string>? Requirements { get; }

		/// <summary>
		/// Creates a schema dependency.
		/// </summary>
		/// <param name="schema">The schema dependency.</param>
		public SchemaOrPropertyList(JsonSchema schema)
		{
			Schema = schema;
		}

		/// <summary>
		/// Creates a property dependency.
		/// </summary>
		/// <param name="requirements">The property dependency.</param>
		public SchemaOrPropertyList(List<string> requirements)
		{
			Requirements = requirements;
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(SchemaOrPropertyList? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(Schema, other.Schema) && Requirements.ContentsEqual(other.Requirements);
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as SchemaOrPropertyList);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				return ((Schema?.GetHashCode() ?? 0) * 397) ^ (Requirements?.GetCollectionHashCode() ?? 0);
			}
		}
	}

	internal class SchemaOrPropertyListJsonConverter : JsonConverter<SchemaOrPropertyList>
	{
		public override SchemaOrPropertyList Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
				return new SchemaOrPropertyList(JsonSerializer.Deserialize<List<string>>(ref reader, options));

			return new SchemaOrPropertyList(JsonSerializer.Deserialize<JsonSchema>(ref reader, options));
		}

		public override void Write(Utf8JsonWriter writer, SchemaOrPropertyList value, JsonSerializerOptions options)
		{
			if (value.Schema != null)
				JsonSerializer.Serialize(writer, value.Schema, options);
			else
				JsonSerializer.Serialize(writer, value.Requirements, options);
		}
	}
}