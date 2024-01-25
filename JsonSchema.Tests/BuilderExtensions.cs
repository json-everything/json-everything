using System;

namespace Json.Schema.Tests;

public static class BuilderExtensions
{
	public static JsonSchemaBuilder MinDate(this JsonSchemaBuilder builder, DateTime date)
	{
		builder.Add(new MinDateKeyword(date));
		return builder;
	}
	public static JsonSchemaBuilder NonVocabMinDate(this JsonSchemaBuilder builder, DateTime date)
	{
		builder.Add(new NonVocabMinDateKeyword(date));
		return builder;
	}
	public static JsonSchemaBuilder MaxDate(this JsonSchemaBuilder builder, DateTime date)
	{
		builder.Add(new MaxDateKeyword(date));
		return builder;
	}
}