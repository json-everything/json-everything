using System;

namespace Json.Path;

/// <summary>
/// Denotes function parameter types.
/// </summary>
[Flags]
public enum ParameterType
{
	/// <summary>
	/// Parameter type unknown
	/// </summary>
	Unspecified,
	/// <summary>
	/// Parameter must be an object
	/// </summary>
	Object,
	/// <summary>
	/// Parameter must be an array
	/// </summary>
	Array,
	/// <summary>
	/// Parameter must be a string
	/// </summary>
	String,
	/// <summary>
	/// Parameter must be a number
	/// </summary>
	Number,
	/// <summary>
	/// Parameter must be either `true` or `false`
	/// </summary>
	Boolean,
	/// <summary>
	/// Parameter must be `null`
	/// </summary>
	Null
}