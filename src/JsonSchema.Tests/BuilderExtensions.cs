using System;
using System.Globalization;

namespace Json.Schema.Tests;

public static class BuilderExtensions
{
	public static JsonSchemaBuilder MinDate(this JsonSchemaBuilder builder, DateTime date)
	{
		var dateString = date.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture);
		builder.Add("minDate", dateString);
		return builder;
	}

	public static JsonSchemaBuilder NonVocabMinDate(this JsonSchemaBuilder builder, DateTime date)
	{
		var dateString = date.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture);
		builder.Add("minDate-nv", dateString);
		return builder;
	}

	public static JsonSchemaBuilder MaxDate(this JsonSchemaBuilder builder, DateTime date)
	{
		var dateString = date.ToString("yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", CultureInfo.InvariantCulture);
		builder.Add("maxDate", dateString);
		return builder;
	}
}