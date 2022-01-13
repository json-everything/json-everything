using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	/// <summary>
	/// Handles `dependentRequired`.
	/// </summary>
	[SchemaPriority(10)]
	[SchemaKeyword(Name)]
	[SchemaDraft(Draft.Draft201909)]
	[SchemaDraft(Draft.Draft202012)]
	[Vocabulary(Vocabularies.Validation201909Id)]
	[Vocabulary(Vocabularies.Validation202012Id)]
	[JsonConverter(typeof(DependentRequiredKeywordJsonConverter))]
	public class DependentRequiredKeyword : IJsonSchemaKeyword, IEquatable<DependentRequiredKeyword>
	{
		internal const string Name = "dependentRequired";

		/// <summary>
		/// The collection of "required"-type dependencies.
		/// </summary>
		public IReadOnlyDictionary<string, IReadOnlyList<string>> Requirements { get; }

		static DependentRequiredKeyword()
		{
			ValidationResults.RegisterConsolidationMethod(ConsolidateAnnotations);
		}
		/// <summary>
		/// Creates a new <see cref="DependentRequiredKeyword"/>.
		/// </summary>
		/// <param name="values">The collection of "required"-type dependencies.</param>
		public DependentRequiredKeyword(IReadOnlyDictionary<string, IReadOnlyList<string>> values)
		{
			Requirements = values ?? throw new ArgumentNullException(nameof(values));
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
			var missingDependencies = new Dictionary<string, List<string>>();
			foreach (var property in Requirements)
			{
				context.Options.LogIndentLevel++;
				context.Log(() => $"Validating property '{property.Key}'.");
				var dependencies = property.Value;
				var name = property.Key;
				if (!context.LocalInstance.TryGetProperty(name, out _))
				{
					context.Log(() => $"Property '{property.Key}' does not exist. Skipping.");
					continue;
				}

				if (!missingDependencies.TryGetValue(name, out var list))
					list = missingDependencies[name] = new List<string>();
				foreach (var dependency in dependencies)
				{
					if (context.LocalInstance.TryGetProperty(dependency, out _)) continue;

					overallResult = false;
					if (context.ApplyOptimizations) break;
					list.Add(dependency);
				}

				if (list.Any())
					context.Log(() => $"Missing properties: [{string.Join(",", list.Select(x => $"'{x}'"))}].");
				else
					context.Log(() => "All dependencies found.");
				context.Options.LogIndentLevel--;
				if (!overallResult && context.ApplyOptimizations) break;
			}

			if (overallResult)
				context.LocalResult.Pass();
			else
			{
				var missing = JsonSerializer.Serialize(missingDependencies);
				context.LocalResult.Fail($"Some required property dependencies are missing: {missing}");
			}
			context.ExitKeyword(Name, context.LocalResult.IsValid);
		}

		private static void ConsolidateAnnotations(ValidationResults localResults)
		{
			var allDependentRequired = localResults.NestedResults.Select(c => c.TryGetAnnotation(Name))
				.Where(a => a != null)
				.Cast<List<string>>()
				.SelectMany(a => a)
				.Distinct()
				.ToList();
			if (localResults.TryGetAnnotation(Name) is List<string> annotation)
				annotation.AddRange(allDependentRequired);
			else if (allDependentRequired.Any())
				localResults.SetAnnotation(Name, allDependentRequired);
		}

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.</returns>
		public bool Equals(DependentRequiredKeyword? other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (Requirements.Count != other.Requirements.Count) return false;
			var byKey = Requirements.Join(other.Requirements,
					td => td.Key,
					od => od.Key,
					(td, od) => new { ThisDef = td.Value, OtherDef = od.Value })
				.ToList();
			if (byKey.Count != Requirements.Count) return false;

			return byKey.All(g => g.ThisDef.ContentsEqual(g.OtherDef));
		}

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The object to compare with the current object.</param>
		/// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as DependentRequiredKeyword);
		}

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>A hash code for the current object.</returns>
		public override int GetHashCode()
		{
			return Requirements.Aggregate(0, (current, obj) =>
			{
				unchecked
				{
					var hashCode = current;
					hashCode = (hashCode * 397) ^ (obj.Key?.GetHashCode() ?? 0);
					hashCode = (hashCode * 397) ^ (obj.Value != null ? obj.Value.GetCollectionHashCode() : 0);
					return hashCode;
				}
			});
		}
	}

	internal class DependentRequiredKeywordJsonConverter : JsonConverter<DependentRequiredKeyword>
	{
		public override DependentRequiredKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var requirements = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(ref reader, options);
			return new DependentRequiredKeyword(requirements.ToDictionary(x => x.Key, x => (IReadOnlyList<string>) x.Value));
		}
		public override void Write(Utf8JsonWriter writer, DependentRequiredKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(DependentRequiredKeyword.Name);
			writer.WriteStartObject();
			foreach (var kvp in value.Requirements)
			{
				writer.WritePropertyName(kvp.Key);
				JsonSerializer.Serialize(writer, kvp.Value, options);
			}
			writer.WriteEndObject();
		}
	}
}