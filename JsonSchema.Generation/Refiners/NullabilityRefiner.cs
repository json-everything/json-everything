using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Refiners;

internal class NullabilityRefiner : ISchemaRefiner
{
	public static NullabilityRefiner Instance { get; } = new();

	private NullabilityRefiner() { }

	public bool ShouldRun(SchemaGenerationContextBase context)
	{
		return context.Intents.OfType<TypeIntent>().Any();
	}

	public void Run(SchemaGenerationContextBase context)
	{
		var typeIntent = context.Intents.OfType<TypeIntent>().FirstOrDefault();
		if (typeIntent == null) return; // shouldn't happen because of ShouldRun(), but including just in case.

		var attributes = context.GetAttributes();

		var nullableAttribute = attributes.OfType<NullableAttribute>().FirstOrDefault();
		var nullabilityOverride = nullableAttribute?.IsNullable;

		if (nullabilityOverride.HasValue)
		{
			if (nullabilityOverride.Value)
				typeIntent.Type |= SchemaValueType.Null;
			else
				typeIntent.Type &= ~SchemaValueType.Null;
			return;
		}

		if (SchemaGeneratorConfiguration.Current.Nullability.HasFlag(Nullability.AllowForNullableValueTypes) &&
			context.Type.IsGenericType && context.Type.GetGenericTypeDefinition() == typeof(Nullable<>))
			typeIntent.Type |= SchemaValueType.Null;

		if (SchemaGeneratorConfiguration.Current.Nullability.HasFlag(Nullability.AllowForReferenceTypes) &&
			// see https://stackoverflow.com/a/16578846/878701
			!context.Type.IsValueType)
			typeIntent.Type |= SchemaValueType.Null;
	}
}