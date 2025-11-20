using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.More;
using Json.Pointer;

namespace Json.Schema;

/// <summary>
/// Handles `$ref`.
/// </summary>
//[SchemaKeyword(Name)]
//[SchemaSpecVersion(SpecVersion.Draft6)]
//[SchemaSpecVersion(SpecVersion.Draft7)]
//[SchemaSpecVersion(SpecVersion.Draft201909)]
//[SchemaSpecVersion(SpecVersion.Draft202012)]
//[SchemaSpecVersion(SpecVersion.DraftNext)]
//[Vocabulary(Vocabularies.Core201909Id)]
//[Vocabulary(Vocabularies.Core202012Id)]
//[Vocabulary(Vocabularies.CoreNextId)]
public class RefKeyword : IKeywordHandler
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "$ref";

	public object? ValidateValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String)
			throw new JsonSchemaException($"'$ref' value must be a string, found {value.ValueKind}");

		if (!Uri.TryCreate(value.GetString(), UriKind.RelativeOrAbsolute, out var uri))
			throw new JsonSchemaException("'$ref' value must be a valid URI");

		return uri;
	}

	public void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
		var reference = (Uri)keyword.Value!;
		var newUri = new Uri(context.BaseUri, reference);

		keyword.Value = newUri;
	}

	internal bool Resolve(KeywordData keyword, BuildContext context)
	{
		var newUri = (Uri)keyword.Value!;
		var fragment = newUri.Fragment;

		JsonSchemaNode? targetSchema;

		if (JsonPointer.TryParse(fragment, out var pointerFragment))
		{
			var targetBase = context.Options.SchemaRegistry.Get(newUri) ??
			                 throw new RefResolutionException(newUri, pointerFragment);

			targetSchema = targetBase.FindSubschema(pointerFragment, context.Options);
		}
		else
		{
			throw new NotImplementedException();
			//var allowLegacy = context.EvaluatingAs is SpecVersion.Draft6 or SpecVersion.Draft7;
			//var anchorFragment = fragment[1..];
			//if ((context.EvaluatingAs <= SpecVersion.Draft201909 && !AnchorKeyword.AnchorPattern201909.IsMatch(anchorFragment)) ||
			//    (context.EvaluatingAs >= SpecVersion.Draft202012 && !AnchorKeyword.AnchorPattern202012.IsMatch(anchorFragment)))
			//	throw new JsonSchemaException($"Unrecognized fragment type `{newUri}`");

			//targetSchema = context.Options.SchemaRegistry.Get(newUri, anchorFragment, allowLegacy) as JsonSchema;
		}

		if (targetSchema == null)
			throw new RefResolutionException(newUri);

		keyword.Subschemas = [targetSchema];

		return true;
	}

	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
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
