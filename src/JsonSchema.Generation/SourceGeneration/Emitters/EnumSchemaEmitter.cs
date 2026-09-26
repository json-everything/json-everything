using System.Text;

namespace Json.Schema.Generation.SourceGeneration.Emitters;

internal class EnumSchemaEmitter : ISchemaEmitter
{
	public bool Handles(TypeInfo type) => type.Kind == TypeKind.Enum;

	/// <remarks>
	/// An enum's own schema is never nullable; <see cref="TypeInfo.IsNullable"/> is set when
	/// the schema is inlined for a nullable property, and null is admitted alongside.
	/// </remarks>
	public void EmitSchema(StringBuilder sb, TypeInfo type, string indent, SchemaEmissionContext context)
	{
		switch (type.EnumFormat)
		{
			case EnumFormat.Values:
				sb.AppendLine();
				sb.Append(type.IsNullable
					? $"{indent}.Type(SchemaValueType.Integer, SchemaValueType.Null)"
					: $"{indent}.Type(SchemaValueType.Integer)");
				break;
			case EnumFormat.NamesAndValues:
				if (type.EnumValues.Count <= 0) return;

				sb.AppendLine();
				sb.Append($"{indent}.AnyOf(");
				sb.AppendLine();
				sb.Append($"{indent}\tnew JsonSchemaBuilder()");
				EmitNames(sb, type, indent + "\t\t", false);
				sb.Append(',');
				sb.AppendLine();
				sb.Append($"{indent}\tnew JsonSchemaBuilder().Type(SchemaValueType.Integer)");
				if (type.IsNullable)
				{
					sb.Append(',');
					sb.AppendLine();
					sb.Append($"{indent}\tnew JsonSchemaBuilder().Type(SchemaValueType.Null)");
				}
				sb.AppendLine();
				sb.Append($"{indent})");
				break;
			default:
				if (type.EnumValues.Count <= 0) return;

				EmitNames(sb, type, indent, type.IsNullable);
				break;
		}
	}

	private static void EmitNames(StringBuilder sb, TypeInfo type, string indent, bool includeNull)
	{
		sb.AppendLine();
		sb.Append($"{indent}.Enum(");

		// Strings alone bind to the `IEnumerable<string>` overload; a null member needs the
		// `JsonNode` overload, so the array is spelled out.
		if (includeNull)
			sb.Append("new global::System.Text.Json.Nodes.JsonNode?[] { ");

		for (int i = 0; i < type.EnumValues.Count; i++)
		{
			if (i > 0)
				sb.Append(", ");
			sb.Append($"\"{CodeEmitterHelpers.EscapeString(type.EnumValues[i])}\"");
		}

		if (includeNull)
			sb.Append(", null }");

		sb.Append(')');
	}
}
