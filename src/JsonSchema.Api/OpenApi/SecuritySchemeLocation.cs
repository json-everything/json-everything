using System.ComponentModel;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Defines the different security scheme locations.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<SecuritySchemeLocation>))]
public enum SecuritySchemeLocation
{
	/// <summary>
	/// Indicates the location is unknown.
	/// </summary>
	Unspecified,
	/// <summary>
	/// Indicates the API key is located in the query.
	/// </summary>
	[Description("query")]
	Query,
	/// <summary>
	/// Indicates the API key is located in a header.
	/// </summary>
	[Description("header")]
	Header,
	/// <summary>
	/// Indicates the API key is located in a cookie.
	/// </summary>
	[Description("cookie")]
	Cookie
}