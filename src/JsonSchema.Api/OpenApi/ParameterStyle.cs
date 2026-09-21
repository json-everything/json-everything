using System.ComponentModel;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Defines the different parameter styles.
/// </summary>
[JsonConverter(typeof(EnumStringConverter<ParameterStyle>))]
public enum ParameterStyle
{
	// see https://spec.openapis.org/oas/v3.1.0#style-values
	/// <summary>
	/// Indicates the parameter type is unknown.
	/// </summary>
	Unspecified,
	/// <summary>
	/// Path-style parameters defined by RFC6570
	/// </summary>
	[Description("matrix")]
	Matrix,
	/// <summary>
	/// Label style parameters defined by RFC6570
	/// </summary>
	[Description("label")]
	Label,
	/// <summary>
	/// Form style parameters defined by RFC6570
	/// </summary>
	[Description("form")]
	Form,
	/// <summary>
	/// Simple style parameters defined by RFC6570
	/// </summary>
	[Description("simple")]
	Simple,
	/// <summary>
	/// Space separated array or object values
	/// </summary>
	[Description("spaceDelimited")]
	SpaceDelimited,
	/// <summary>
	/// Pipe separated array or object values
	/// </summary>
	[Description("pipeDelimited")]
	PipeDelimited,
	/// <summary>
	/// Provides a simple way of rendering nested objects using form parameters
	/// </summary>
	[Description("deepObject")]
	DeepObject
}