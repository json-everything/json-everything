using System;
using System.Linq;
using System.Text.Json.Serialization;
using Json.Schema.Generation.Intents;
#pragma warning disable IL2075
#pragma warning disable IL3050

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
			var fieldInfo = context.Type.GetField(enumMember.ToString()!);
			var fieldAttributes = fieldInfo?.GetCustomAttributes(true).ToList();

			// JsonIgnoreCondition values other than Never and Always don't make sense in the context of JSON schema generation,
			// so we only ignore members if they are marked as "always ignored."
			var ignoreAttribute =
				(Attribute?)fieldAttributes?.OfType<JsonIgnoreAttribute>().FirstOrDefault(a => a.Condition == JsonIgnoreCondition.Always) ??
				fieldAttributes?.OfType<JsonExcludeAttribute>().FirstOrDefault();

			return ignoreAttribute == null;
		};

		string GetName(object enumMember)
		{
#if NET9_0_OR_GREATER
			var fieldInfo = context.Type.GetField(enumMember.ToString()!);
			var fieldAttributes = fieldInfo?.GetCustomAttributes(true).ToList();

			var enumNameAttribute = fieldAttributes?.OfType<JsonStringEnumMemberNameAttribute>().FirstOrDefault();

			return enumNameAttribute?.Name ?? enumMember.ToString()!;
#else
			return enumMember.ToString()!;
#endif
		}

		var includedValues = Enum.GetValues(context.Type)
			.Cast<object>()
			.Where(ShouldIncludeMember)
			.Select(GetName)
			.ToList();
		context.Intents.Add(new EnumIntent(includedValues));
	}
}