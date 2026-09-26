using System.Collections.Generic;

namespace Json.Schema.Api.Analyzer;

/// <summary>
/// A parameter carried in the route, query string, or headers.
/// </summary>
internal sealed class EndpointParameterInfo
{
	/// <summary>
	/// Gets or sets the parameter name as it appears in the request.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets where the parameter is carried: `path`, `query`, or `header`.
	/// </summary>
	public string Location { get; set; } = "query";

	/// <summary>
	/// Gets or sets whether the parameter must be present.
	/// </summary>
	public bool Required { get; set; }

	/// <summary>
	/// Gets or sets the fully-qualified type name, when one could be determined.
	/// </summary>
	public string? TypeName { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }
}

/// <summary>
/// A response an operation can produce.
/// </summary>
internal sealed class EndpointResponseInfo
{
	/// <summary>
	/// Gets or sets the HTTP status code.
	/// </summary>
	public int StatusCode { get; set; } = 200;

	/// <summary>
	/// Gets or sets the fully-qualified payload type name, or null when the response
	/// carries no body or the type could not be determined.
	/// </summary>
	public string? TypeName { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string Description { get; set; } = string.Empty;
}

/// <summary>
/// One operation discovered in the API surface.
/// </summary>
internal sealed class EndpointInfo
{
	/// <summary>
	/// Gets or sets the route template, with a leading slash.
	/// </summary>
	public string Route { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the lower-case HTTP method.
	/// </summary>
	public string Method { get; set; } = "get";

	/// <summary>
	/// Gets the names of the descriptions the operation appears in.  Empty means the default
	/// description.
	/// </summary>
	public List<string> DocumentNames { get; } = [];

	/// <summary>
	/// Gets or sets the operation ID, when one could be determined.
	/// </summary>
	public string? OperationId { get; set; }

	/// <summary>
	/// Gets or sets the summary.
	/// </summary>
	public string? Summary { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Gets the tags.
	/// </summary>
	public List<string> Tags { get; } = [];

	/// <summary>
	/// Gets or sets the fully-qualified request body type name, when the operation has one.
	/// </summary>
	public string? RequestBodyTypeName { get; set; }

	/// <summary>
	/// Gets or sets the name of the parameter the request body binds to, so that its
	/// documentation comment can be found.
	/// </summary>
	public string? RequestBodyParameterName { get; set; }

	/// <summary>
	/// Gets or sets the request body description.
	/// </summary>
	public string? RequestBodyDescription { get; set; }

	/// <summary>
	/// Gets or sets whether the request body type is validated, and therefore whether the
	/// validation failure response applies.
	/// </summary>
	public bool RequestBodyIsValidated { get; set; }

	/// <summary>
	/// Gets the parameters.
	/// </summary>
	public List<EndpointParameterInfo> Parameters { get; } = [];

	/// <summary>
	/// Gets the responses.
	/// </summary>
	public List<EndpointResponseInfo> Responses { get; } = [];
}
