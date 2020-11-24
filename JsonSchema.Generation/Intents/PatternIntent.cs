using JetBrains.Annotations;

namespace Json.Schema.Generation.Intents
{
	internal class PatternIntent : ISchemaKeywordIntent
	{
		public string Value { get; }

		public PatternIntent([RegexPattern] string value)
		{
			Value = value;
		}

		public void Apply(JsonSchemaBuilder builder)
		{
			builder.Pattern(Value);
		}
	}
}