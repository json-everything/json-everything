using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.More;
using Json.Pointer;

namespace Json.Schema.Experiments;

public class DynamicRefKeywordHandler : IKeywordHandler
{
	private readonly bool _requireLocalAnchor;

	private static readonly Regex _anchorPattern202012 = new("^[A-Za-z_][-A-Za-z0-9._]*$");

	public static DynamicRefKeywordHandler RequireAdjacentAnchor { get; } = new(true);
	public static DynamicRefKeywordHandler Instance { get; } = new(false);

	public string Name => "$dynamicRef";
	public string[]? Dependencies { get; }

	private DynamicRefKeywordHandler(bool requireLocalAnchor)
	{
		_requireLocalAnchor = requireLocalAnchor;
	}

	public KeywordEvaluation Handle(JsonNode? keywordValue, EvaluationContext context, IReadOnlyCollection<KeywordEvaluation> evaluations)
	{
		var reference = (keywordValue as JsonValue)?.GetString();
		if (reference is null || !Uri.TryCreate(reference, UriKind.RelativeOrAbsolute, out _))
			throw new SchemaValidationException("$dynamicRef must contain a valid URI reference", context);

		var newUri = new Uri(context.BaseUri, reference);
		var fragment = newUri.Fragment;

		var newBaseUri = new Uri(newUri.GetLeftPart(UriPartial.Query));

		JsonNode? target;
		if (JsonPointer.TryParse(fragment, out var pointer))
		{
			var targetBase = context.Options.SchemaRegistry.Get(newBaseUri);
			if (!pointer.TryEvaluate(targetBase, out target))
				throw new RefResolutionException(newBaseUri, pointer);
		}
		else
		{
			var anchor = fragment[1..];
			if (!_anchorPattern202012.IsMatch(anchor))
				throw new SchemaValidationException($"Unrecognized fragment type `{newUri}`", context);

			(target, newBaseUri) = context.Options.SchemaRegistry.Get(context.DynamicScope, newBaseUri, anchor, _requireLocalAnchor);
		}

		var localContext = context;
		localContext.RefUri = newBaseUri;
		localContext.EvaluationPath = localContext.EvaluationPath.Combine(Name);
		localContext.SchemaLocation = localContext.SchemaLocation.Combine(Name);

		var result = localContext.Evaluate(target);

		return new KeywordEvaluation
		{
			Valid = result.Valid,
			Children = [result]
		};
	}

	IEnumerable<JsonNode?> IKeywordHandler.GetSubschemas(JsonNode? keywordValue) => [];
}