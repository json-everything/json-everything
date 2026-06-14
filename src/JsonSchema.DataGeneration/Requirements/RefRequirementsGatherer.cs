using System;
using Json.Schema.Keywords;

namespace Json.Schema.DataGeneration.Requirements;

internal class RefRequirementsGatherer : IRequirementsGatherer
{
	private const SchemaValueType _primitiveTypes = SchemaValueType.Null | SchemaValueType.String | SchemaValueType.Integer | SchemaValueType.Number | SchemaValueType.Boolean;

	public void AddRequirements(RequirementsContext context, JsonSchemaNode schema)
	{
		var refKeyword = schema.GetKeyword<RefKeyword>();
		if (refKeyword != null)
		{
			if (refKeyword.Subschemas is null or { Length: 0 })
				throw new RefResolutionException((Uri)refKeyword.Value!);

			context.And(GetTargetRequirements(context, refKeyword.Subschemas[0]));
			return;
		}

		var dynamicRefKeyword = schema.GetKeyword("$dynamicRef");
		if (dynamicRefKeyword == null) return;
		if (!TryGetDynamicAnchor(dynamicRefKeyword, out var anchor) || !context.TryResolveDynamicAnchor(anchor, out var dynamicTarget))
		{
			context.And(new RequirementsContext { IsFalse = true });
			return;
		}

		context.And(GetTargetRequirements(context, dynamicTarget));
	}

	private static RequirementsContext GetTargetRequirements(RequirementsContext context, JsonSchemaNode target)
	{
		if (context.RemainingRefDepth > 0)
			return target.GetRequirements(context.CreateRefBranchContext());

		return BuildCutoffFallbackRequirements(target);
	}

	private static bool TryGetDynamicAnchor(KeywordData dynamicRefKeyword, out string anchor)
	{
		anchor = string.Empty;
		if (dynamicRefKeyword.RawValue.ValueKind != System.Text.Json.JsonValueKind.String) return false;

		var rawReference = dynamicRefKeyword.RawValue.GetString();
		if (string.IsNullOrWhiteSpace(rawReference)) return false;

		if (rawReference![0] == '#')
		{
			if (rawReference.Length < 2 || rawReference[1] == '/') return false;

			anchor = rawReference[1..];
			return true;
		}

		if (!Uri.TryCreate(rawReference, UriKind.RelativeOrAbsolute, out var uri)) return false;

		var fragment = uri.Fragment;
		if (fragment.Length < 2 || fragment[0] != '#' || fragment[1] == '/') return false;

		anchor = fragment[1..];
		return true;
	}

	private static RequirementsContext BuildCutoffFallbackRequirements(JsonSchemaNode target)
	{
		var fallback = new RequirementsContext
		{
			ReachedRefDepthCutoff = true
		};

		var typeKeyword = target.GetKeyword<TypeKeyword>();
		if (typeKeyword == null)
		{
			fallback.IsFalse = true;
			return fallback;
		}

		var allowedTypes = (SchemaValueType)typeKeyword.Value!;
		var primitiveTypes = allowedTypes & _primitiveTypes;
		if (primitiveTypes == 0)
		{
			fallback.IsFalse = true;
			return fallback;
		}

		fallback.Type = primitiveTypes.HasFlag(SchemaValueType.Null)
			? SchemaValueType.Null
			: primitiveTypes.HasFlag(SchemaValueType.String)
				? SchemaValueType.String
				: primitiveTypes.HasFlag(SchemaValueType.Integer)
					? SchemaValueType.Integer
					: primitiveTypes.HasFlag(SchemaValueType.Number)
						? SchemaValueType.Number
						: SchemaValueType.Boolean;

		return fallback;
	}
}