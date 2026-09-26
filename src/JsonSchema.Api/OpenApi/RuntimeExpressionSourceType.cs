namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Defines the different runtime expression sources.
/// </summary>
public enum RuntimeExpressionSourceType
{
	/// <summary>
	/// Indicates the expression source type is unknown.
	/// </summary>
	Unspecified,
	/// <summary>
	/// Indicates the expression source is the header.
	/// </summary>
	Header,
	/// <summary>
	/// Indicates the expression source is the query string.
	/// </summary>
	Query,
	/// <summary>
	/// Indicates the expression source is the path.
	/// </summary>
	Path,
	/// <summary>
	/// Indicates the expression source is the body.
	/// </summary>
	Body
}