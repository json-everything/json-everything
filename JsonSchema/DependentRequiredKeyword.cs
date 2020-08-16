using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaPriority(10)]
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(DependentRequiredKeywordJsonConverter))]
	public class DependentRequiredKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "dependentRequired";

		public IReadOnlyDictionary<string, List<string>> Requirements { get; }

		static DependentRequiredKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}

		public DependentRequiredKeyword(IReadOnlyDictionary<string, List<string>> values)
		{
			Requirements = values;
		}

		public void Validate(ValidationContext context)
		{
			if (context.LocalInstance.ValueKind != JsonValueKind.Object)
			{
				context.IsValid = true;
				return;
			}

			var overallResult = true;
			var missingDependencies = new Dictionary<string, List<string>>();
			foreach (var property in Requirements)
			{
				var dependencies = property.Value;
				var name = property.Key;
				if (!context.LocalInstance.TryGetProperty(name, out _)) continue;

				foreach (var dependency in dependencies)
				{
					if (context.LocalInstance.TryGetProperty(dependency, out _)) continue;

					overallResult = false;
					if (!missingDependencies.TryGetValue(name, out var list)) 
						list = missingDependencies[name] = new List<string>();
					list.Add(dependency);
				}
			}

			context.IsValid = overallResult;
			if (!context.IsValid)
				context.Message = $"Some required property dependencies are missing: {JsonSerializer.Serialize(missingDependencies)}";
		}

		private static void ConsolidateAnnotations(IEnumerable<ValidationContext> sourceContexts, ValidationContext destContext)
		{
			var allDependentRequired = sourceContexts.Select(c => c.TryGetAnnotation(Name))
				.Where(a => a != null)
				.Cast<List<string>>()
				.SelectMany(a => a)
				.Distinct()
				.ToList();
			if (destContext.TryGetAnnotation(Name) is List<string> annotation)
				annotation.AddRange(allDependentRequired);
			else
				destContext.Annotations[Name] = allDependentRequired;
		}
	}

	public class DependentRequiredKeywordJsonConverter : JsonConverter<DependentRequiredKeyword>
	{
		public override DependentRequiredKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType != JsonTokenType.StartObject)
				throw new JsonException("Expected object");

			var requirements = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(ref reader, options);
			return new DependentRequiredKeyword(requirements);
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