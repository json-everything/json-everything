using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Generators;

internal class ObjectSchemaGenerator : ISchemaGenerator
{
	public bool Handles(Type type)
	{
		return true;
	}

	public void AddConstraints(SchemaGenerationContextBase context)
	{
		context.Intents.Add(new TypeIntent(SchemaValueType.Object));

		var props = new Dictionary<string, SchemaGenerationContextBase>();
		var required = new List<string>();
		var propertiesToGenerate = context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
		var fieldsToGenerate = context.Type.GetFields(BindingFlags.Public | BindingFlags.Instance);
		var hiddenPropertiesToGenerate = context.Type.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(p => p.GetCustomAttribute<JsonIncludeAttribute>() != null);
		var hiddenFieldsToGenerate = context.Type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
			.Where(p => p.GetCustomAttribute<JsonIncludeAttribute>() != null);
		var membersToGenerate = propertiesToGenerate.Cast<MemberInfo>()
			.Concat(fieldsToGenerate)
			.Concat(hiddenPropertiesToGenerate)
			.Concat(hiddenFieldsToGenerate);

		membersToGenerate = SchemaGeneratorConfiguration.Current.PropertyOrder switch
		{
			PropertyOrder.AsDeclared => membersToGenerate.OrderBy(m => m, context.DeclarationOrderComparer),
			PropertyOrder.ByName => membersToGenerate.OrderBy(m => m.Name),
			_ => membersToGenerate
		};

		foreach (var member in membersToGenerate)
		{
			var memberAttributes = member.GetCustomAttributes().ToList();
			var ignoreAttribute = (Attribute?)memberAttributes.OfType<JsonIgnoreAttribute>().FirstOrDefault(a=>a.Condition == JsonIgnoreCondition.Always) ??
								  memberAttributes.OfType<JsonExcludeAttribute>().FirstOrDefault();
			if (ignoreAttribute != null) continue;

			var unconditionalAttributes = memberAttributes.OfType<SchemaGenerationAttribute>().Where(x => x.ConditionGroup == null).Cast<Attribute>().ToList();
			var conditionalAttributes = memberAttributes.Except(unconditionalAttributes).ToList();

			if (member.IsReadOnly() && !unconditionalAttributes.OfType<ReadOnlyAttribute>().Any())
				unconditionalAttributes.Add(new ReadOnlyAttribute(true));

			if (member.IsWriteOnly() && !unconditionalAttributes.OfType<WriteOnlyAttribute>().Any())
				unconditionalAttributes.Add(new WriteOnlyAttribute(true));

			var memberContext = SchemaGenerationContextCache.Get(member.GetMemberType(), unconditionalAttributes);

			var name = SchemaGeneratorConfiguration.Current.PropertyNamingMethod(member.Name);
			var nameAttribute = unconditionalAttributes.OfType<JsonPropertyNameAttribute>().FirstOrDefault();
			if (nameAttribute != null)
				name = nameAttribute.Name;

			if (unconditionalAttributes.OfType<ObsoleteAttribute>().Any())
			{
				if (memberContext is TypeGenerationContext)
					memberContext = new MemberGenerationContext(memberContext, new List<Attribute>());
				memberContext.Intents.Add(new DeprecatedIntent(true));
			}

			props.Add(name, memberContext);

			if (unconditionalAttributes.OfType<RequiredAttribute>().Any())
				required.Add(name);
		}


		if (props.Count > 0)
		{
			context.Intents.Add(new PropertiesIntent(props));

			if (required.Count > 0)
				context.Intents.Add(new RequiredIntent(required));
		}
	}

}