namespace Json.Schema.Analysis;

public static class JsonSchemaKeywordHelpers
{
	public static JsonSchemaBuilder DiagnosticMessage(this JsonSchemaBuilder builder, string message)
	{
		builder.Unrecognized(Diagnostic.MessageKeyword, message);

		return builder;
	}
	public static JsonSchemaBuilder DiagnosticTarget(this JsonSchemaBuilder builder, string message)
	{
		builder.Unrecognized(Diagnostic.TargetKeyword, message);

		return builder;
	}
}