using System;
using System.Text.Json;

namespace Json.Schema.Keywords.Draft201909;

/// <summary>
/// Handles `$recursiveRef`.
/// </summary>
/// <remarks>
/// This keyword is used to create a recursive reference to a schema.
/// </remarks>
public class RecursiveRefKeyword : RefKeyword
{
	/// <summary>
	/// Gets the singleton instance of the <see cref="RecursiveRefKeyword"/>.
	/// </summary>
	public new static RecursiveRefKeyword Instance { get; } = new();

	/// <summary>
	/// Gets the name of the handled keyword.
	/// </summary>
	public override string Name => "$recursiveRef";

	/// <summary>
	/// Initializes a new instance of the <see cref="RecursiveRefKeyword"/> class.
	/// </summary>
	protected RecursiveRefKeyword()
	{
	}

	/// <summary>
	/// Validates the specified JSON element as a keyword value and optionally returns a value to be shared across the other methods.
	/// </summary>
	/// <param name="value">The JSON element to validate and convert. Represents the value to be checked for keyword compliance.</param>
	/// <returns>An object that is shared with the other methods.  This object is saved to <see cref="KeywordData.Value"/>.</returns>
	public override object? ValidateKeywordValue(JsonElement value)
	{
		if (value.ValueKind != JsonValueKind.String || !value.ValueEquals("#"))
			throw new JsonSchemaException($"'{Name}' value must be a string with value '#', found {value.ValueKind}");

		if (!Uri.TryCreate(value.GetString(), UriKind.RelativeOrAbsolute, out var uri))
			throw new JsonSchemaException($"'{Name}' value must be a valid URI");

		return uri;
	}

	internal override bool TryResolve(KeywordData keyword, BuildContext context)
	{
		var recursiveTarget = context.Options.SchemaRegistry.GetRecursive(context.BaseUri);
		if (recursiveTarget is not null)
		{
			// don't set the subschema because we may need to go deeper into the scope
			keyword.Value = null;
			return false;
		}

		return base.TryResolve(keyword, context);
	}

	/// <summary>
	/// Evaluates the specified keyword using the provided evaluation context and returns the result of the evaluation.
	/// </summary>
	/// <param name="keyword">The keyword data to be evaluated. Cannot be null.</param>
	/// <param name="context">The context in which the keyword evaluation is performed. Cannot be null.</param>
	/// <returns>A KeywordEvaluation object containing the results of the evaluation.</returns>
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