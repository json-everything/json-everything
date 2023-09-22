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
			.Concat(hiddenFieldsToGenerate)
			.ToList();

		membersToGenerate = SchemaGeneratorConfiguration.Current.PropertyOrder switch
		{
			PropertyOrder.AsDeclared => membersToGenerate.OrderBy(m => m, context.DeclarationOrderComparer).ToList(),
			PropertyOrder.ByName => membersToGenerate.OrderBy(m => m.Name).ToList(),
			_ => membersToGenerate
		};

		var conditionalAttributes = new Dictionary<object, List<(MemberInfo, ConditionalAttribute)>>();
		var strictPropertyDefinitions = new Dictionary<(object, string), SchemaGenerationContextBase>();
		var addUnevaluatedProperties = false;

		foreach (var member in membersToGenerate)
		{
			var memberAttributes = member.GetCustomAttributes().ToList();
			var ignoreAttribute = (Attribute?)memberAttributes.OfType<JsonIgnoreAttribute>().FirstOrDefault(a=>a.Condition == JsonIgnoreCondition.Always) ??
								  memberAttributes.OfType<JsonExcludeAttribute>().FirstOrDefault();
			if (ignoreAttribute != null) continue;

			var unconditionalAttributes = memberAttributes.Where(x => x is not ConditionalAttribute sga || sga.ConditionGroup == null).ToList();
			var localConditionalAttributes = memberAttributes.Except(unconditionalAttributes).OfType<ConditionalAttribute>().ToList();
			foreach (var conditions in localConditionalAttributes.GroupBy(x => x.ConditionGroup))
			{
				if (!conditionalAttributes.TryGetValue(conditions.Key!, out var list)) 
					conditionalAttributes[conditions.Key!] = list = new List<(MemberInfo, ConditionalAttribute)>();

				list.AddRange(conditions.Select(x => (member, x)));
			}

			if (member.IsReadOnly() && !unconditionalAttributes.OfType<ReadOnlyAttribute>().Any())
				unconditionalAttributes.Add(new ReadOnlyAttribute(true));

			if (member.IsWriteOnly() && !unconditionalAttributes.OfType<WriteOnlyAttribute>().Any())
				unconditionalAttributes.Add(new WriteOnlyAttribute(true));

			var memberContext = SchemaGenerationContextCache.Get(member.GetMemberType(), unconditionalAttributes);

			var name = SchemaGeneratorConfiguration.Current.PropertyNameResolver(member);

			if (unconditionalAttributes.OfType<ObsoleteAttribute>().Any())
			{
				if (memberContext is TypeGenerationContext)
					memberContext = new MemberGenerationContext(memberContext, new List<Attribute>());
				memberContext.Intents.Add(new DeprecatedIntent(true));
			}

			if (SchemaGeneratorConfiguration.Current.StrictConditionals &&
			    localConditionalAttributes.Any())
			{
				addUnevaluatedProperties = true;
				var applicableConditionGroups = localConditionalAttributes.Select(x => x.ConditionGroup).Distinct();
				foreach (var conditionGroup in applicableConditionGroups)
				{
					strictPropertyDefinitions.Add((conditionGroup!, name), memberContext);
				}
			}
			else
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
			.OfType<IConditionAttribute>()
			.SelectMany(x => ExpandEnumConditions(x, membersToGenerate))
			.GroupBy(x => x.Item1.ConditionGroup)
			.ToList();

		if (!conditionGroups.Any()) return;

		if (conditionGroups.Count == 1)
		{
			// add directly to schema
			var conditionKey = conditionGroups[0].Key!;
			if (conditionalAttributes.TryGetValue(conditionKey, out var consequences))
			{
				var strictProperties = strictPropertyDefinitions.Where(x => Equals(x.Key.Item1, conditionKey))
					.ToDictionary(x => x.Key.Item2, x => x.Value);
				var thenSubschema = GenerateThen(consequences, strictProperties);

				if (thenSubschema != null)
				{
					context.Intents.Add(GenerateIf(conditionGroups[0]));
					context.Intents.Add(thenSubschema);
				}
			}
		}
		else
		{
			// wrap in allOf
			var allOf = new AllOfIntent();
			foreach (var conditionGroup in conditionGroups)
			{
				var conditionKey = conditionGroup.Key!;
				if (conditionalAttributes.TryGetValue(conditionKey, out var consequences))
				{
					var strictProperties = strictPropertyDefinitions.Where(x => Equals(x.Key.Item1, conditionKey))
						.ToDictionary(x => x.Key.Item2, x => x.Value);
					var thenSubschema = GenerateThen(consequences, strictProperties);

					if (thenSubschema != null)
						allOf.Subschemas.Add(new ISchemaKeywordIntent[]
						{
							GenerateIf(conditionGroup),
							thenSubschema
						});
				}
			}
			if (allOf.Subschemas.Any())
				context.Intents.Add(allOf);
		}

		if (addUnevaluatedProperties)
			context.Intents.Add(new UnevaluatedPropertiesIntent());
	}

	private static IEnumerable<(IConditionAttribute, MemberInfo)> ExpandEnumConditions(IConditionAttribute condition, IEnumerable<MemberInfo> members)
	{
		var member = members.FirstOrDefault(x => x.Name == condition.PropertyName);
		if (member == null) yield break;

		var memberType = member.GetMemberType();
		switch (condition)
		{
			case IfMinAttribute min:
				min.PropertyType = memberType;
				break;
			case IfMaxAttribute max:
				max.PropertyType = memberType;
				break;
			case IfEnumAttribute when !memberType.IsEnum:
				yield break;
			case IfEnumAttribute ifEnumAttribute:
				var values = Enum.GetValues(memberType);
				foreach (var value in values)
				{
					yield return (new IfAttribute(ifEnumAttribute.PropertyName, ifEnumAttribute.UseNumbers ? value : value.ToString(), value), member);
				}

				yield break;
		}
		yield return (condition, member);
	}

	private static IfIntent GenerateIf(IEnumerable<(IConditionAttribute Attribute, MemberInfo Member)> conditions)
	{
		var properties = new Dictionary<string, SchemaGenerationContextBase>();
		var required = new List<string>();
		foreach (var condition in conditions)
		{
			var name = SchemaGeneratorConfiguration.Current.PropertyNameResolver(condition.Member);
			if (!properties.TryGetValue(name, out var context))
				properties[name] = context = new AdHocGenerationContext();
			switch (condition.Attribute)
			{
				case IfAttribute ifAtt:
					context.Intents.Add(new ConstIntent(ifAtt.Value));
					break;
				case IfMinAttribute ifMin:
					var minIntent = ifMin.GetIntent();
					if (minIntent == null) continue;
					context.Intents.Add(minIntent);
					break;
				case IfMaxAttribute ifMax:
					var maxIntent = ifMax.GetIntent();
					if (maxIntent == null) continue;
					context.Intents.Add(maxIntent);
					break;
			}
			if (!required.Contains(name))
				required.Add(name);
		}

		var ifIntent = new IfIntent(new ISchemaKeywordIntent[]
		{
			new PropertiesIntent(properties),
			new RequiredIntent(required)
		});

		return ifIntent;
	}

	private static ThenIntent? GenerateThen(List<(MemberInfo member, ConditionalAttribute attribute)> consequences,
		Dictionary<string, SchemaGenerationContextBase> prebuiltMemberContexts)
	{
		var applicable = consequences.Where(x => x.attribute is IAttributeHandler); // should be all
		var required = consequences.Where(x => x.attribute is RequiredAttribute)
			.Select(x => ((RequiredAttribute)x.attribute).PropertyName)
			.ToList();
		var properties = prebuiltMemberContexts;
		foreach (var consequence in applicable.GroupBy(x => x.member))
		{
			var name = SchemaGeneratorConfiguration.Current.PropertyNameResolver(consequence.Key);
			if (properties.TryGetValue(name, out var localContext))
				localContext = new MemberGenerationContext(localContext, new List<Attribute>());
			else
			{
				var type = consequence.Key.GetMemberType();
				localContext = new TypeGenerationContext(type);
			}
			foreach (var (_, attribute) in consequence)
			{
				((IAttributeHandler)attribute).AddConstraints(localContext, attribute);
			}

			properties[name] = localContext;
		}

		var thenIntents = new List<ISchemaKeywordIntent>();

		if (properties.Any())
			thenIntents.Add(new PropertiesIntent(properties));

		if (required.Any())
			thenIntents.Add(new RequiredIntent(required));

		if (!thenIntents.Any()) return null;

		var thenIntent = new ThenIntent(thenIntents);

		return thenIntent;
	}
}