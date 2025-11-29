using System.Linq;
using System.Text.Json;
using Json.More;

namespace Json.Schema.Keywords;

/// <summary>
/// Handles `enum`.
/// </summary>
public class EnumKeyword : IKeywordHandler
{
	public static EnumKeyword Instance { get; set; } = new();

	/// <summary>
	/// The JSON name of the keyword.
	/// </summary>
	public string Name => "enum";

	protected EnumKeyword()
	{
	}

	public object? ValidateKeywordValue(JsonElement value)
	{
		return null;
	}

	public void BuildSubschemas(KeywordData keyword, BuildContext context)
	{
	}

	public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
	{
		if (!keyword.RawValue.EnumerateArray().Any(x => x.IsEquivalentTo(context.Instance)))
		{
			return new KeywordEvaluation
			{
				IsValid = false,
				Keyword = Name,
				Error = ErrorMessages.GetEnum(context.Options.Culture)
					.ReplaceToken("received", context.Instance, JsonSchemaSerializerContext.Default.JsonElement)
					.ReplaceToken("values", keyword.RawValue!, JsonSchemaSerializerContext.Default.JsonElement)
			};
		}

		return new KeywordEvaluation
		{
			IsValid = true,
			Keyword = Name
		};
	}
}
