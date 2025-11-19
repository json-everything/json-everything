using System;
using System.Text.Json;

namespace Json.Schema;

/// <summary>
/// Handles `$id`.
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
public class IdKeyword : IKeywordHandler //, IIdKeyword
{
	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "$id";

	public object? ValidateValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String || 
		    !Uri.TryCreate(value.GetString(), UriKind.RelativeOrAbsolute, out var uri))
			throw new JsonSchemaException("$id requires a string in the format of a URI");

		return uri;
	}

	public void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		return new KeywordEvaluation
		{
			Keyword = Name,
			IsValid = true
		};
	}
}
