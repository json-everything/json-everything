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

		var conditionalAttributes = new Dictionary<object, List<SchemaGenerationAttribute>>();

		foreach (var member in membersToGenerate)
		{
			var memberAttributes = member.GetCustomAttributes().ToList();
			var ignoreAttribute = (Attribute?)memberAttributes.OfType<JsonIgnoreAttribute>().FirstOrDefault(a=>a.Condition == JsonIgnoreCondition.Always) ??
								  memberAttributes.OfType<JsonExcludeAttribute>().FirstOrDefault();
			if (ignoreAttribute != null) continue;

			var unconditionalAttributes = memberAttributes.Where(x => x is not SchemaGenerationAttribute sga || sga.ConditionGroup == null).ToList();
			var localConditionalAttributes = memberAttributes.Except(unconditionalAttributes).OfType<SchemaGenerationAttribute>().ToList();
			foreach (var conditions in localConditionalAttributes.GroupBy(x => x.ConditionGroup))
			{
				if (!conditionalAttributes.TryGetValue(conditions.Key!, out var list)) 
					conditionalAttributes[conditions.Key!] = list = new List<SchemaGenerationAttribute>();

				list.AddRange(conditions);
			}

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

			foreach (var conditionalRequiredAttribute in localConditionalAttributes.OfType<RequiredAttribute>())
			{
				conditionalRequiredAttribute.PropertyName = name;
			}
		}

		if (props.Count > 0)
		{
			context.Intents.Add(new PropertiesIntent(props));

			if (required.Count > 0)
				context.Intents.Add(new RequiredIntent(required));
		}

		var conditionGroups = context.Type.GetCustomAttributes()
			.Where(x => x is IfAttribute or IfEnumAttribute)
			.Cast<SchemaGenerationAttribute>()
			.SelectMany(x => ExpandEnumConditions(x, membersToGenerate))
			.GroupBy(x => x.ConditionGroup)
			.ToList();

		if (conditionGroups.Count == 1)
		{
			// add directly to schema
			var conditionKey = conditionGroups[0].Key!;
			if (conditionalAttributes.TryGetValue(conditionKey, out var consequences))
			{
				context.Intents.Add(GenerateIf(conditionGroups[0]));
				context.Intents.Add(GenerateThen(consequences));
			}
		}
		else
		{
			// wrap in anyOf
			var anyOf = new AnyOfIntent();
			foreach (var conditionGroup in conditionGroups)
			{
				var conditionKey = conditionGroup.Key!;
				if (conditionalAttributes.TryGetValue(conditionKey, out var consequences))
				{
					anyOf.Subschemas.Add(new ISchemaKeywordIntent[]
					{
						GenerateIf(conditionGroup),
						GenerateThen(consequences)
					});
				}
			}
			if (anyOf.Subschemas.Any())
				context.Intents.Add(anyOf);
		}
	}

	private static IEnumerable<IfAttribute> ExpandEnumConditions(SchemaGenerationAttribute conditionGroup, IEnumerable<MemberInfo> members)
	{
		if (conditionGroup is IfAttribute ifAttribute) yield return ifAttribute;

		if (conditionGroup is IfEnumAttribute ifEnumAttribute)
		{
			var member = members.FirstOrDefault(x => x.Name == ifEnumAttribute.PropertyName);
			if (member == null) yield break;

			var memberType = member!.GetMemberType();
			if (!memberType.IsEnum) yield break;

			var values = Enum.GetValues(memberType);
			foreach (var value in values)
			{
				yield return new IfAttribute(ifEnumAttribute.PropertyName, ifEnumAttribute.UseNumbers ? value : value.ToString(), value);
			}
		}
	}

	private static IfIntent GenerateIf(IEnumerable<IfAttribute> conditions)
	{
		var properties = new Dictionary<string, SchemaGenerationContextBase>();
		var required = new List<string>();
		foreach (var condition in conditions)
		{
			// TODO: This needs to be the configured name.
			properties.Add(condition.PropertyName, new AdHocGenerationContext
			{
				Intents = { new ConstIntent(condition.Value) }
			});

			required.Add(condition.PropertyName);
		}

		var ifIntent = new IfIntent(new ISchemaKeywordIntent[]
		{
			new PropertiesIntent(properties),
			new RequiredIntent(required)
		});

		return ifIntent;
	}

	private static ThenIntent GenerateThen(IEnumerable<SchemaGenerationAttribute> consequences)
	{
		var context = new AdHocGenerationContext();
		var applicable = consequences.OfType<IAttributeHandler>(); // should be all
		var required = consequences.OfType<RequiredAttribute>().Select(x => x.PropertyName).ToList();
		foreach (var consequence in applicable)
		{
			consequence.AddConstraints(context, (SchemaGenerationAttribute)consequence);
		}

		if (required.Any())
			context.Intents.Add(new RequiredIntent(required));

		var thenIntent = new ThenIntent(context.Intents);

		return thenIntent;
	}
}