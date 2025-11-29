using System;
using System.Linq;
using System.Text.Json;
using Json.Pointer;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `$ref`.
/// </summary>
public class RefKeyword : IKeywordHandler
{
	public static RefKeyword Instance { get; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public virtual string Name => "$ref";

	protected RefKeyword()
	{
	}

	public virtual object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String)
			throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

		if (!Uri.TryCreate(value.GetString(), UriKind.RelativeOrAbsolute, out var uri))
			throw new JsonSchemaException($"'{Name}' value must be a valid URI");

		return uri;
	}

	public virtual void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var reference = (Uri)keyword.Value!;
		var newUri = new Uri(context.BaseUri, reference);

		keyword.Value = newUri;
	}

	internal virtual void TryResolve(KeywordData keyword, BuildContext context)
	{
		var newUri = (Uri?)keyword.Value;
		if (newUri is null) return;

		var fragment = newUri.Fragment;

		JsonSchemaNode? targetSchema;
		if (JsonPointer.TryParse(fragment, out var pointerFragment))
		{
			var targetBase = context.Options.SchemaRegistry.Get(newUri);

			targetSchema = targetBase?.FindSubschema(pointerFragment, context);
		}
		else
		{
			var anchorFragment = fragment[1..]; // drop #
			targetSchema = context.Options.SchemaRegistry.Get(newUri, anchorFragment);
		}

		if (targetSchema is not null)
			keyword.Subschemas = [targetSchema];
	}

	public virtual KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		var newUri = (Uri)keyword.Value!;
		var subschema = keyword.Subschemas.FirstOrDefault();
		if (subschema is null)
			throw new RefResolutionException(newUri);

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
}
