using System.ComponentModel;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Defines the different parameter locations.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<ParameterLocation>))]
public enum ParameterLocation
{
	/// <summary>
	/// Indicates the location is unknown.
	/// </summary>
	Unspecified,
	/// <summary>
	/// Indicates the parameter is in the query string.
	/// </summary>
	[Description("query")]
	Query,
	/// <summary>
	/// Indicates the parameter is in a header.
	/// </summary>
	[Description("header")]
	Header,
	/// <summary>
	/// Indicates the parameter is in the path.
	/// </summary>
	[Description("path")]
	Path,
	/// <summary>
	/// Indicates the parameter is in a cookie.
	/// </summary>
	[Description("cookie")]
	Cookie
}