using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Json.Schema.Generation.SourceGeneration.Emitters;

internal static class ConditionalSchemaEmitter
{
	public static void EmitConditionals(StringBuilder sb, TypeInfo type, string indent, SchemaEmissionContext context)
	{
		var conditionalsWithConsequences = type.Conditionals
			.Where(c => c.PropertyConsequences.Count > 0)
			.ToList();
		
		if (conditionalsWithConsequences.Count == 0) return;
		if (conditionalsWithConsequences.Count == 1)
		{
			var conditional = conditionalsWithConsequences[0];
			EmitIfClause(sb, conditional, indent);
			EmitThenClause(sb, conditional, type, indent, context);
		}
		else
		{
			sb.AppendLine();
			sb.Append($"{indent}.AllOf(");
			
			for (int i = 0; i < conditionalsWithConsequences.Count; i++)
			{
				var conditional = conditionalsWithConsequences[i];
				sb.AppendLine();
				sb.Append($"{indent}\tnew JsonSchemaBuilder()");
				
				EmitIfClause(sb, conditional, indent + "\t");
				EmitThenClause(sb, conditional, type, indent + "\t", context);
				
				if (i < conditionalsWithConsequences.Count - 1)
					sb.Append(",");
			}
			
			sb.AppendLine();
			sb.Append($"{indent})");
		}
	}

	private static void EmitIfClause(StringBuilder sb, ConditionalInfo conditional, string indent)
	{
		sb.AppendLine();
		sb.Append($"{indent}.If(new JsonSchemaBuilder()");

		if (conditional.Triggers.Count > 0)
		{
			var triggerGroups = conditional.Triggers
				.GroupBy(t => t.PropertySchemaName)
				.ToList();

			sb.AppendLine();
			sb.Append($"{indent}\t.Properties(");
			sb.AppendLine();
			
			for (int i = 0; i < triggerGroups.Count; i++)
			{
				var triggerGroup = triggerGroups[i];
				sb.Append($"{indent}\t\t(\"{CodeEmitterHelpers.EscapeString(triggerGroup.Key)}\", ");
				EmitTriggerSchema(sb, triggerGroup.ToList());
				sb.Append(")");
				
				if (i < triggerGroups.Count - 1)
					sb.Append(",");
				sb.AppendLine();
			}
			
			sb.Append($"{indent}\t)");
			
			sb.AppendLine();
			sb.Append($"{indent}\t.Required(");
			for (int i = 0; i < triggerGroups.Count; i++)
			{
				if (i > 0)
					sb.Append(", ");
				sb.Append($"\"{CodeEmitterHelpers.EscapeString(triggerGroups[i].Key)}\"");
			}
			sb.Append(")");
		}

		sb.AppendLine();
		sb.Append($"{indent})");
	}

	private static void EmitTriggerSchema(StringBuilder sb, IReadOnlyList<ConditionalTrigger> triggers)
	{
		sb.Append("new JsonSchemaBuilder()");

		var equalityValues = triggers
			.Where(t => t.Type is ConditionalTriggerType.Equality or ConditionalTriggerType.Enum)
			.Select(t => t.ExpectedValue)
			.Where(v => v != null)
			.Distinct()
			.ToList();

		if (equalityValues.Count == 1)
			sb.Append($".Const({equalityValues[0]})");
		else if (equalityValues.Count > 1)
			sb.Append($".Enum({string.Join(", ", equalityValues)})");

		foreach (var trigger in triggers.Where(t => t.Type is ConditionalTriggerType.Minimum or ConditionalTriggerType.Maximum))
		{
			switch (trigger.Type)
			{
				case ConditionalTriggerType.Minimum:
					var minKeyword = trigger.IsExclusive ? "ExclusiveMinimum" : "Minimum";
					sb.Append($".{minKeyword}({trigger.NumericValue})");
					break;
				case ConditionalTriggerType.Maximum:
					var maxKeyword = trigger.IsExclusive ? "ExclusiveMaximum" : "Maximum";
					sb.Append($".{maxKeyword}({trigger.NumericValue})");
					break;
			}
		}
	}

	private static void EmitThenClause(StringBuilder sb, ConditionalInfo conditional, TypeInfo type, string indent, SchemaEmissionContext context)
	{
		if (conditional.PropertyConsequences.Count == 0) return;

		sb.AppendLine();
		sb.Append($"{indent}.Then(new JsonSchemaBuilder()");

		var propertiesWithAttributes = conditional.PropertyConsequences
			.Where(c => c.ConditionalAttributes.Count > 0 || c.IsConditionallyReadOnly || c.IsConditionallyWriteOnly)
			.ToList();

		if (propertiesWithAttributes.Count > 0)
		{
			sb.AppendLine();
			sb.Append($"{indent}\t.Properties(");
			sb.AppendLine();

			for (int i = 0; i < propertiesWithAttributes.Count; i++)
			{
				var prop = propertiesWithAttributes[i];
				sb.Append($"{indent}\t\t(\"{CodeEmitterHelpers.EscapeString(prop.PropertySchemaName)}\", ");

				if (type.StrictConditionals)
				{
					var fullProp = type.Properties.FirstOrDefault(p => p.SchemaName == prop.PropertySchemaName);
					if (fullProp != null)
					{
						PropertySchemaEmitter.EmitPropertySchema(sb, fullProp, indent + "\t\t", context);

						EmitConditionalAttributes(sb, prop.ConditionalAttributes, $"{indent}\t\t\t");

						if (prop.IsConditionallyReadOnly)
						{
							sb.AppendLine();
							sb.Append($"{indent}\t\t\t.ReadOnly(true)");
						}
						if (prop.IsConditionallyWriteOnly)
						{
							sb.AppendLine();
							sb.Append($"{indent}\t\t\t.WriteOnly(true)");
						}
					}
					else
					{
						sb.Append("new JsonSchemaBuilder()");

						EmitConditionalAttributes(sb, prop.ConditionalAttributes, $"{indent}\t\t\t");
					}
				}
				else
				{
					sb.Append("new JsonSchemaBuilder()");

					EmitConditionalAttributes(sb, prop.ConditionalAttributes, $"{indent}\t\t\t");

					if (prop.IsConditionallyReadOnly)
					{
						sb.AppendLine();
						sb.Append($"{indent}\t\t\t.ReadOnly(true)");
					}
					if (prop.IsConditionallyWriteOnly)
					{
						sb.AppendLine();
						sb.Append($"{indent}\t\t\t.WriteOnly(true)");
					}
				}

				sb.Append(")");

				if (i < propertiesWithAttributes.Count - 1)
					sb.Append(",");
				sb.AppendLine();
			}

			sb.Append($"{indent}\t)");
		}

		var requiredProps = conditional.PropertyConsequences
			.Where(c => c.IsConditionallyRequired)
			.ToList();

		if (requiredProps.Count > 0)
		{
			sb.AppendLine();
			sb.Append($"{indent}\t.Required(");
			
			for (int i = 0; i < requiredProps.Count; i++)
			{
				if (i > 0)
					sb.Append(", ");
				sb.Append($"\"{CodeEmitterHelpers.EscapeString(requiredProps[i].PropertySchemaName)}\"");
			}
			
			sb.Append(")");
		}

		sb.AppendLine();
		sb.Append($"{indent})");
	}

	/// <summary>
	/// Emits the constraints a condition group applies to one property.
	/// </summary>
	/// <remarks>
	/// A custom attribute carries its own emitter, so it is emitted as a call to that rather
	/// than through the built-in constraint switch — the same split the unconditional path
	/// makes.  Filtering to built-ins here dropped custom attributes from the consequence.
	/// </remarks>
	private static void EmitConditionalAttributes(
		StringBuilder sb,
		List<AttributeInfo> attributes,
		string indent)
	{
		foreach (var attr in attributes)
		{
			if (attr is { IsCustomEmitter: true, AttributeFullName: not null })
			{
				sb.AppendLine();
				sb.Append(indent);
				SchemaCodeEmitter.EmitCustomAttributeCall(sb, attr);
				continue;
			}

			if (!SchemaCodeEmitter.ShouldEmitBuiltInAttribute(attr)) continue;

			sb.AppendLine();
			sb.Append(indent);
			SchemaCodeEmitter.EmitAttributeConstraint(sb, attr);
		}
	}
}
