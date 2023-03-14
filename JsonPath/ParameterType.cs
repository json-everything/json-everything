using System;

namespace Json.Path;

/// <summary>
/// Denotes function parameter types.
/// </summary>
[Flags]
public enum ParameterType
{
	/// <summary>
	/// Parameter type unknown.
	/// </summary>
	Unspecified,
	/// <summary>
	/// Parameter must be an object.
	/// </summary>
	Object,
	/// <summary>
	/// Parameter must be an array.
	/// </summary>
	Array,
	/// <summary>
	/// Parameter must be a string.
	/// </summary>
	String,
	/// <summary>
	/// Parameter must be a number.
	/// </summary>
	Number,
	/// <summary>
	/// Parameter must be either `true` or `false`.
	/// </summary>
	Boolean,
	/// <summary>
	/// Parameter must be `null`.
	/// </summary>
	Null,
	/// <summary>
	/// Parameter may be any JSON value or `Nothing`.
	/// </summary>
	Value = Object | Array | String | Number | Boolean | Null,
	/// <summary>
	/// Parameter must be a logical value (non-JSON true/false).
	/// </summary>
	Logical,
	/// <summary>
	/// Parameter must be a nodelist (result of a query).
	/// </summary>
	Nodelist
}