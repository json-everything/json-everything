using System;
using System.Linq;
using System.Text.Json.Serialization;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Generators;

internal class EnumGenerator : ISchemaGenerator
{
	public bool Handles(Type type)
	{
		return type.IsEnum;
	}

	public void AddConstraints(SchemaGenerationContextBase context)
	{
		var values = Enum.GetNames(context.Type).ToList();
		bool ShouldIncludeMember(string enumValue)
		{
			var fieldInfo = context.Type.GetField(enumValue);
			var fieldAttributes = fieldInfo?.GetCustomAttributes(inherit: true)?.ToList();

			// JsonIgnoreCondition values other than Never and Always don't make sense in the context of JSON schema generation,
			// so we only ignore members if they are marked as "always ignored."
			var ignoreAttribute =
				(Attribute?)fieldAttributes?.OfType<JsonIgnoreAttribute>().FirstOrDefault(a => a.Condition == JsonIgnoreCondition.Always) ??
				fieldAttributes?.OfType<JsonExcludeAttribute>().FirstOrDefault();

			return ignoreAttribute == null;
		};

		var vs = values.Where(ShouldIncludeMember);

		context.Intents.Add(new EnumIntent(vs));
	}
}