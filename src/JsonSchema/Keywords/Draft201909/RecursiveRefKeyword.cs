using System;
using System.Text.Json;

namespace Json.Schema.Keywords.Draft201909;

/// <summary>
/// Handles `$recursiveRef`.
/// </summary>
public class RecursiveRefKeyword : RefKeyword
{
	public static RecursiveRefKeyword Instance { get; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public override string Name => "$recursiveRef";

	protected RecursiveRefKeyword()
	{
	}

	public override object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String || !value.ValueEquals("#"))
			throw new JsonSchemaException($"'{Name}' value must be a string with value '#', found {value.ValueKind}");

		if (!Uri.TryCreate(value.GetString(), UriKind.RelativeOrAbsolute, out var uri))
			throw new JsonSchemaException($"'{Name}' value must be a valid URI");

		return uri;
	}

	internal override void TryResolve(KeywordData keyword, BuildContext context)
	{
		var recursiveTarget = context.Options.SchemaRegistry.GetRecursive(context.BaseUri);
		if (recursiveTarget is not null)
		{
			// don't set the subschema because we may need to go deeper into the scope
			keyword.Value = null;
			return;
		}

		base.TryResolve(keyword, context);
	}

	public override KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		var recursiveRef = keyword.Value is null;
		if (recursiveRef)
		{
			var subschema = context.SchemaRegistry.GetRecursive(context.Scope);
			if (subschema is null)
				throw new RefResolutionException(context.Scope.LocalScope, string.Empty, AnchorType.Recursive);

			var refContext = context with
			{
				EvaluationPath = context.EvaluationPath.Combine(Name)
			};

			var subschemaEvaluation = subschema.Evaluate(refContext);
			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = subschemaEvaluation.IsValid,
				Details = [subschemaEvaluation]
			};
		}

		// behaves like $ref
		return base.Evaluate(keyword, context);
	}
}