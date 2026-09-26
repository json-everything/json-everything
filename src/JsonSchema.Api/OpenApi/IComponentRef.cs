using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Indicates that the item is a reference to another object rather than being the object itself.
/// </summary>
internal interface IComponentRef
{
	/// <summary>
	/// The URI for the reference.
	/// </summary>
	Uri Ref { get; }
	/// <summary>
	/// Gets or sets the summary.
	/// </summary>
	string? Summary { get; }
	/// <summary>
	/// Gets or sets the description.
	/// </summary>
	string? Description { get; }
	/// <summary>
	/// Gets whether the reference has been resolved.
	/// </summary>
	bool IsResolved { get; }

	/// <summary>
	/// Resolves the reference.
	/// </summary>
	/// <param name="root">The document root.</param>
	/// <param name="options">Serializer options</param>
	/// <returns>A task.</returns>
	Task Resolve(OpenApiDocument root, JsonSerializerOptions? options);
}