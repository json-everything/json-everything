using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Schema.Generation.Intents;
//#pragma warning disable IL2075

namespace Json.Schema.Generation.Generators;

internal class EnumGenerator : ISchemaGenerator
{
	public bool Handles(Type type)
	{
		return type.IsEnum;
	}

	public void AddConstraints(SchemaGenerationContextBase context)
	{
		bool ShouldIncludeMember(object enumMember)
		{
			var fieldInfo = context.Type.GetField(enumMember.ToString());
			var fieldAttributes = fieldInfo?.GetCustomAttributes(inherit: true).ToList();

			// JsonIgnoreCondition values other than Never and Always don't make sense in the context of JSON schema generation,
			// so we only ignore members if they are marked as "always ignored."
			var ignoreAttribute =
				(Attribute?)fieldAttributes?.OfType<JsonIgnoreAttribute>().FirstOrDefault(a => a.Condition == JsonIgnoreCondition.Always) ??
				fieldAttributes?.OfType<JsonExcludeAttribute>().FirstOrDefault();

			return ignoreAttribute == null;
		};

		var includedValues = Enum.GetValues(context.Type)
			.Cast<object>()
			.Where(ShouldIncludeMember)
			.Select(x =>
			{
				var serialized = JsonSerializer.SerializeToNode(x, SchemaGeneratorConfiguration.Current.SerializerOptions)!; // includes JSON quotes
				var unencodedValue = serialized.AsValue().GetString();
				return unencodedValue;
			})
			.ToList();
		context.Intents.Add(new EnumIntent(includedValues));
	}
}