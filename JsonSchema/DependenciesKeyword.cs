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
	[JsonConverter(typeof(DependenciesKeywordJsonConverter))]
	public class DependenciesKeyword : IJsonSchemaKeyword, IRefResolvable
	{
		internal const string Name = "dependencies";

		public IReadOnlyDictionary<string, SchemaOrPropertyList> Requirements { get; }

		static DependenciesKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}

		public DependenciesKeyword(IReadOnlyDictionary<string, SchemaOrPropertyList> values)
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
			var evaluatedProperties = new List<string>();
			foreach (var property in Requirements)
			{
				var requirements = property.Value;
				var name = property.Key;
				if (!context.LocalInstance.TryGetProperty(name, out _)) continue;

				if (requirements.Schema != null)
				{
					var subContext = ValidationContext.From(context,
						subschemaLocation: context.SchemaLocation.Combine(PointerSegment.Create($"{name}")));
					requirements.Schema.ValidateSubschema(subContext);
					overallResult &= subContext.IsValid;
					context.NestedContexts.Add(subContext);
					if (subContext.IsValid)
						evaluatedProperties.Add(name);
				}
				else
				{
					var missingDependencies = new List<string>();
					foreach (var dependency in requirements.Requirements)
					{
						if (context.LocalInstance.TryGetProperty(dependency, out _)) continue;

						overallResult = false;
						missingDependencies.Add(dependency);
					}

					if (!missingDependencies.Any())
						evaluatedProperties.Add(name);
					else
						overallResult = false;
				}
			}

			context.IsValid = overallResult;
			if (!context.IsValid)
				context.Message = $"The following properties failed their dependent schemas: {JsonSerializer.Serialize(evaluatedProperties)}";
		}

		private static void ConsolidateAnnotations(IEnumerable<ValidationContext> sourceContexts, ValidationContext destContext)
		{
			var allDependencies = sourceContexts.Select(c => c.TryGetAnnotation(Name))
				.Where(a => a != null)
				.Cast<List<string>>()
				.SelectMany(a => a)
				.Distinct()
				.ToList();
			if (destContext.TryGetAnnotation(Name) is List<string> annotation)
				annotation.AddRange(allDependencies);
			else
				destContext.Annotations[Name] = allDependencies;
		}

		public IRefResolvable ResolvePointerSegment(string value)
		{
			if (!Requirements.TryGetValue(value, out var entry)) return null;

			return entry.Schema;
		}
	}

	public class DependenciesKeywordJsonConverter : JsonConverter<DependenciesKeyword>
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

	[JsonConverter(typeof(SchemaOrPropertyListJsonConverter))]
	public class SchemaOrPropertyList
	{
		public JsonSchema Schema { get; set; }
		public List<string> Requirements { get; set; }
	}

	public class SchemaOrPropertyListJsonConverter : JsonConverter<SchemaOrPropertyList>
	{
		public override SchemaOrPropertyList Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.StartArray)
				return new SchemaOrPropertyList
				{
					Requirements = JsonSerializer.Deserialize<List<string>>(ref reader, options)
				};

			return new SchemaOrPropertyList
			{
				Schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options)
			};
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