namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Defines the different types of runtime expressions.
/// </summary>
public enum RuntimeExpressionType
{
	/// <summary>
	/// Indicates the expression type is unknown.
	/// </summary>
	Unspecified,
	/// <summary>
	/// Indicates a `$url` expression.
	/// </summary>
	Url,
	/// <summary>
	/// Indicates a `$method` expression.
	/// </summary>
	Method,
	/// <summary>
	/// Indicates a `$statusCode` expression.
	/// </summary>
	StatusCode,
	/// <summary>
	/// Indicates a `$request` expression.
	/// </summary>
	Request,
	/// <summary>
	/// Indicates a `$response` expression.
	/// </summary>
	Response
}