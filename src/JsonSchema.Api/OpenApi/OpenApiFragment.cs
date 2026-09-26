using System;
using System.Collections.Generic;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// One assembly's contribution to an OpenAPI document.
/// </summary>
/// <remarks>
/// Each project that references this package emits a fragment describing the API surface
/// it declares.  <c>AddOpenApi()</c> collects the fragments from the entry assembly and
/// everything it references, then assembles them into a single document, so controllers
/// declared in a class library are included without that library knowing about the host.
/// </remarks>
public sealed class OpenApiFragment
{
	/// <summary>
	/// Gets or sets the name of the description this fragment contributes to, or null for
	/// the default description.
	/// </summary>
	/// <remarks>
	/// An assembly emits one fragment per description it contributes to, so assembling a
	/// description means collecting the fragments carrying its name.  An endpoint placed in
	/// several descriptions appears in each fragment.
	/// </remarks>
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the operations declared by this assembly.
	/// </summary>
	public IReadOnlyList<OpenApiFragmentOperation> Operations { get; set; } = [];

	/// <summary>
	/// Gets or sets the component schemas declared by this assembly, keyed by component name.
	/// </summary>
	public IReadOnlyDictionary<string, JsonSchema> Schemas { get; set; } =
		new Dictionary<string, JsonSchema>();

	/// <summary>
	/// Gets or sets the component name for each type with a schema.
	/// </summary>
	public IReadOnlyDictionary<Type, string> ComponentNames { get; set; } =
		new Dictionary<Type, string>();
}

/// <summary>
/// One operation within an <see cref="OpenApiFragment"/>.
/// </summary>
public sealed class OpenApiFragmentOperation
{
	/// <summary>
	/// Gets or sets the route template.
	/// </summary>
	public string Route { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets the HTTP method, lower-case.
	/// </summary>
	public string Method { get; set; } = "get";

	/// <summary>
	/// Gets or sets the operation ID.
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
	/// Gets or sets the tags.
	/// </summary>
	public IReadOnlyList<string> Tags { get; set; } = [];

	/// <summary>
	/// Gets or sets the request body type, when the operation has one.
	/// </summary>
	public Type? RequestBodyType { get; set; }

	/// <summary>
	/// Gets or sets the request body description.
	/// </summary>
	public string? RequestBodyDescription { get; set; }

	/// <summary>
	/// Gets or sets whether the request body is validated, and therefore whether the
	/// validation failure response applies.
	/// </summary>
	public bool RequestBodyIsValidated { get; set; }

	/// <summary>
	/// Gets or sets the parameters.
	/// </summary>
	public IReadOnlyList<OpenApiFragmentParameter> Parameters { get; set; } = [];

	/// <summary>
	/// Gets or sets the responses.
	/// </summary>
	public IReadOnlyList<OpenApiFragmentResponse> Responses { get; set; } = [];
}

/// <summary>
/// A parameter within an <see cref="OpenApiFragmentOperation"/>.
/// </summary>
public sealed class OpenApiFragmentParameter
{
	/// <summary>
	/// Gets or sets the parameter name.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Gets or sets where the parameter is carried: `path`, `query`, or `header`.
	/// </summary>
	public string Location { get; set; } = "query";

	/// <summary>
	/// Gets or sets whether the parameter is required.
	/// </summary>
	public bool Required { get; set; }

	/// <summary>
	/// Gets or sets the parameter type.
	/// </summary>
	public Type? Type { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string? Description { get; set; }
}

/// <summary>
/// A response within an <see cref="OpenApiFragmentOperation"/>.
/// </summary>
public sealed class OpenApiFragmentResponse
{
	/// <summary>
	/// Gets or sets the status code.
	/// </summary>
	public int StatusCode { get; set; } = 200;

	/// <summary>
	/// Gets or sets the payload type, or null when the response carries no body.
	/// </summary>
	public Type? Type { get; set; }

	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	public string Description { get; set; } = string.Empty;
}
