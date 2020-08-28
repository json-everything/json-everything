using System;

namespace Json.Schema.Tests
{
	public static class BuilderExtensions
	{
		public static JsonSchemaBuilder MinDate(this JsonSchemaBuilder builder, DateTime date)
		{
			builder.Add(new VocabularyTests.MinDateKeyword(date));
			return builder;
		}
		public static JsonSchemaBuilder MaxDate(this JsonSchemaBuilder builder, DateTime date)
		{
			builder.Add(new VocabularyTests.MaxDateKeyword(date));
			return builder;
		}
	}
}