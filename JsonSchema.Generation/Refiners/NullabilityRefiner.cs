using System;
using System.Collections.Generic;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation.Refiners
{
	/// <summary>
	/// Refines schemas
	/// </summary>
	public class NullabilityRefiner : ISchemaRefiner
	{
		public static NullabilityRefiner Instance { get; } = new NullabilityRefiner();

		private NullabilityRefiner(){}

		/// <summary>
		/// Determines if the refiner should run.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public bool ShouldRun(SchemaGeneratorContext context)
		{
			return context.Intents.OfType<TypeIntent>().Any();
		}

		/// <summary>
		/// Runs the refiner.
		/// </summary>
		/// <param name="context"></param>
		public void Run(SchemaGeneratorContext context)
		{
			var typeIntent = context.Intents.OfType<TypeIntent>().FirstOrDefault();
			if (typeIntent == null) return; // shouldn't happen because of ShouldRun(), but including just in case.

			var nullableAttribute = context.Attributes.OfType<NullableAttribute>().FirstOrDefault();
			var nullabilityOverride = nullableAttribute?.IsNullable;

			if (context.Configuration.Nullability.HasFlag(Nullability.AllowForNullableValueTypes) &&
			    context.Type.IsGenericType && context.Type.GetGenericTypeDefinition() == typeof(Nullable<>) &&
			    nullabilityOverride == true)
				typeIntent.Type |= SchemaValueType.Null;

			if (context.Configuration.Nullability.HasFlag(Nullability.AllowForReferenceTypes) &&
			    context.Type.IsByRef)
				typeIntent.Type |= SchemaValueType.Null;
		}
	}
}
