using System.Collections.Generic;
using System.Text;
using Json.Schema.Generation.SourceGeneration;
using Json.Schema.Generation.SourceGeneration.Emitters;
using System.Linq;
using TypeInfo = Json.Schema.Generation.SourceGeneration.TypeInfo;

namespace Json.Schema.Api.Analyzer;

/// <summary>
/// Emits the schemas that populate an OpenAPI document's `components/schemas`.
/// </summary>
/// <remarks>
/// This reuses the schema generator's emitter pipeline: the emitters produce only the
/// schema body (`.Type()`, `.Properties()`, `.Required()`, …) and resolve every `$ref`
/// through <see cref="SchemaEmissionContext"/>'s type-id map.  Supplying a map of
/// component pointers is therefore all that is needed to retarget the refs — no `$id`
/// and no `$schema` are emitted, which is what an OpenAPI-internal schema wants.
/// </remarks>
internal static class ComponentSchemaEmitter
{
	/// <summary>
	/// Builds the map from type key to component pointer.
	/// </summary>
	/// <param name="types">The types to include as components.</param>
	/// <returns>A map suitable for <see cref="SchemaEmissionContext"/>.</returns>
	public static Dictionary<string, string> BuildComponentIds(IReadOnlyList<TypeInfo> types)
	{
		var ids = new Dictionary<string, string>();

		foreach (var type in types)
		{
			var key = SchemaEmissionContext.GetTypeKey(type.TypeSymbol);
			ids[key] = $"#/components/schemas/{ComponentNaming.ForType(type.FullyQualifiedName)}";
		}

		return ids;
	}

	/// <summary>
	/// Emits the builder expression for a single component schema.
	/// </summary>
	/// <param name="type">The type to emit.</param>
	/// <param name="componentIds">The component pointer map.</param>
	/// <param name="indent">The indent for continuation lines.</param>
	/// <returns>The emitted expression, without a trailing `.Build()`.</returns>
	public static string EmitSchema(TypeInfo type, Dictionary<string, string> componentIds, string indent)
	{
		var sb = new StringBuilder();
		sb.Append("new JsonSchemaBuilder()");

		var context = new SchemaEmissionContext(componentIds)
		{
			RootType = type.TypeSymbol
		};

		var emitter = SchemaEmitterRegistry.Emitters.FirstOrDefault(x => x.Handles(type));
		emitter?.EmitSchema(sb, type, indent, context);

		// Type-level attributes (`[AdditionalProperties]`, `[Title]`, …) are applied by the
		// caller in the schema generator, not by the emitters, so they are applied here too.
		SchemaCodeEmitter.EmitAttributes(sb, type.TypeAttributes, indent);

		return sb.ToString();
	}
}
