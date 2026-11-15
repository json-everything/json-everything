using System;
using System.Globalization;
using System.Text.Json;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Tests;

/// <summary>
/// Tests that verify custom keywords can be implemented and added into a dialect.
/// </summary>
public class CustomDialectTests
{
	public class MinDateKeyword : IKeywordHandler
	{
		public static readonly MinDateKeyword Instance = new();

		public string Name => "minDate";

		private MinDateKeyword()
		{
		}

		public object? ValidateKeywordValue(JsonElement value)
		{
			if (value.ValueKind is not JsonValueKind.String)
				throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

			return DateTime.Parse(value.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
		}

		public void BuildSubschemas(KeywordData keyword, BuildContext context)
		{
		}

		public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
		{
			if (context.Instance.ValueKind is not JsonValueKind.String) return KeywordEvaluation.Ignore;

			var dateString = context.Instance.GetString();
			var date = DateTime.Parse(dateString!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
			var expectedDate = (DateTime)keyword.Value!;

			if (date < expectedDate)
			{
				return new KeywordEvaluation
				{
					Keyword = Name,
					IsValid = false,
					Error = $"Date must be on or after {expectedDate:O}"
				};
			}

			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};
		}
	}

	public class Draft4ExclusiveMinimumKeyword : IKeywordHandler
	{
		public static readonly Draft4ExclusiveMinimumKeyword Instance = new();

		public string Name => "exclusiveMinimum";

		private Draft4ExclusiveMinimumKeyword()
		{
		}

		public object? ValidateKeywordValue(JsonElement value)
		{
			if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
				throw new JsonSchemaException($"'{Name}' value must be a boolean, found {value.ValueKind}");

			return value.GetBoolean();
		}

		public void BuildSubschemas(KeywordData keyword, BuildContext context)
		{
			// Store the minimum value if present in sibling keywords
			if (context.LocalSchema.TryGetProperty("minimum", out var minimumValue))
			{
				keyword.Value = ((bool)keyword.Value!, minimumValue);
			}
		}

		public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
		{
			if (context.Instance.ValueKind is not JsonValueKind.Number) return KeywordEvaluation.Ignore;

			var (isExclusive, minimum) = ((bool, JsonElement))keyword.Value!;
			if (!isExclusive)
			{
				return new KeywordEvaluation
				{
					Keyword = Name,
					IsValid = true
				};
			}

			var comparison = JsonMath.NumberCompare(context.Instance, minimum);
			if (comparison == 0)
			{
				return new KeywordEvaluation
				{
					Keyword = Name,
					IsValid = false,
					Error = "minimum is exclusive"
				};
			}

			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};
		}
	}

	[Test]
	public void CustomKeywordInDialect_ValidatesCorrectly()
	{
		var customDialect = Dialect.Draft201909.With([MinDateKeyword.Instance]);
		var buildOptions = new BuildOptions { Dialect = customDialect };

		var futureDate = DateTime.UtcNow.AddDays(1);
		var builder = new JsonSchemaBuilder(buildOptions);
		builder.Add("minDate", futureDate.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture));
		var schema = builder.Build();

		var validInstance = JsonDocument.Parse($"\"{DateTime.UtcNow.AddDays(2):O}\"").RootElement;
		var invalidInstance = JsonDocument.Parse($"\"{DateTime.UtcNow:O}\"").RootElement;

		var validResult = schema.Evaluate(validInstance);
		var invalidResult = schema.Evaluate(invalidInstance);

		validResult.AssertValid();
		invalidResult.AssertInvalid();
	}

	[TestCase(3, false)]
	[TestCase(8, true)]
	[TestCase(5, false)]
	[TestCase(5.1, true)]
	public void Draft4ExclusiveMinimum_CustomDialect(decimal instanceValue, bool isValid)
	{
		var draft4Dialect = Dialect.Draft202012.With([Draft4ExclusiveMinimumKeyword.Instance]);

		var schemaText = """
		                 {
		                 	"minimum": 5,
		                 	"exclusiveMinimum": true
		                 }
		                 """;
		var schema = JsonSchema.FromText(schemaText, new BuildOptions { Dialect = draft4Dialect });

		var instance = JsonDocument.Parse(instanceValue.ToString(CultureInfo.InvariantCulture)).RootElement;

		var result = schema.Evaluate(instance);

		Assert.That(result.IsValid, Is.EqualTo(isValid));
	}

	[Test]
	public void MultipleCustomKeywords_InSameDialect()
	{
		var maxDateKeyword = new MaxDateKeyword();
		var customDialect = Dialect.Draft201909.With([MinDateKeyword.Instance, maxDateKeyword]);
		var buildOptions = new BuildOptions { Dialect = customDialect };

		var minDate = DateTime.UtcNow;
		var maxDate = DateTime.UtcNow.AddDays(10);

		var builder = new JsonSchemaBuilder(buildOptions);
		builder.Add("minDate", minDate.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture));
		builder.Add("maxDate", maxDate.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture));
		var schema = builder.Build();

		var validInstance = JsonDocument.Parse($"\"{DateTime.UtcNow.AddDays(5):O}\"").RootElement;
		var tooEarlyInstance = JsonDocument.Parse($"\"{DateTime.UtcNow.AddDays(-1):O}\"").RootElement;
		var tooLateInstance = JsonDocument.Parse($"\"{DateTime.UtcNow.AddDays(11):O}\"").RootElement;

		var validResult = schema.Evaluate(validInstance);
		var tooEarlyResult = schema.Evaluate(tooEarlyInstance);
		var tooLateResult = schema.Evaluate(tooLateInstance);

		validResult.AssertValid();
		tooEarlyResult.AssertInvalid();
		tooLateResult.AssertInvalid();
	}

	private class MaxDateKeyword : IKeywordHandler
	{
		public string Name => "maxDate";

		public object? ValidateKeywordValue(JsonElement value)
		{
			if (value.ValueKind is not JsonValueKind.String)
				throw new JsonSchemaException($"'{Name}' value must be a string, found {value.ValueKind}");

			return DateTime.Parse(value.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
		}

		public void BuildSubschemas(KeywordData keyword, BuildContext context)
		{
		}

		public KeywordEvaluation Evaluate(KeywordData keyword, EvaluationContext context)
		{
			if (context.Instance.ValueKind is not JsonValueKind.String) return KeywordEvaluation.Ignore;

			var dateString = context.Instance.GetString();
			var date = DateTime.Parse(dateString!, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
			var expectedDate = (DateTime)keyword.Value!;

			if (date > expectedDate)
			{
				return new KeywordEvaluation
				{
					Keyword = Name,
					IsValid = false,
					Error = $"Date must be on or before {expectedDate:O}"
				};
			}

			return new KeywordEvaluation
			{
				Keyword = Name,
				IsValid = true
			};
		}
	}
}
