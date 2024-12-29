using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Refiners;

internal class NullabilityRefiner : ISchemaRefiner
{
	public static NullabilityRefiner Instance { get; } = new();

	private NullabilityRefiner() { }

	public bool ShouldRun(SchemaGenerationContextBase context)
	{
		return false;

		//return context.Intents.OfType<TypeIntent>().Any() ||
		//       context.GetAttributes().OfType<NullableAttribute>().Any() ||
		//       context.Type.IsNullableValueType();
	}

	public void Run(SchemaGenerationContextBase context)
	{
		var typeIntent = context.Intents.OfType<TypeIntent>().FirstOrDefault();
		if (typeIntent == null)
		{
			// could be enum or a member context with attributes
		}

		var attributes = context.GetAttributes();

		var nullableAttribute = attributes.OfType<NullableAttribute>().FirstOrDefault();
		var nullabilityOverride = nullableAttribute?.IsNullable;

		var typeIsNullableValueType = context.Type.IsNullableValueType();

		if (nullabilityOverride.HasValue)
		{
			if (nullabilityOverride.Value)
			{
				if (typeIsNullableValueType)
					ReplaceTypeWithNullableIntent(context, typeIntent);
				else
					typeIntent.Type |= SchemaValueType.Null;
			}
			else
				typeIntent.Type &= ~SchemaValueType.Null;
			return;
		}
	}

	private static void ReplaceTypeWithNullableIntent(SchemaGenerationContextBase context, TypeIntent typeIntent)
	{
		var nullableTypeIntent = new TypeIntent(typeIntent.Type | SchemaValueType.Null);
		var index = context.Intents.IndexOf(typeIntent);
		context.Intents[index] = nullableTypeIntent;
	}
}