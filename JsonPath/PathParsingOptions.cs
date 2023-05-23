using System;

namespace Json.Path;

/// <summary>
/// Defines a set of configuration options to control parsing behavior.
/// </summary>
public class PathParsingOptions
{
	/// <summary>
	/// Gets or sets whether mathematical operators are allowed.
	/// </summary>
	/// <remarks>
	/// These operators are an extension of the specification, so they
	/// are disallowed by default.
	/// </remarks>
	public bool AllowMathOperations { get; set; }

	/// <summary>
	/// Gets or sets whether a JSON Path can start with the `@` symbol
	/// instead of `$`.
	/// </summary>
	public bool AllowRelativePathStart { get; set; }

	/// <summary>
	/// Gets or sets whether JSON objects and arrays (double-quotes only)
	/// are permitted in expression syntax.
	/// </summary>
	/// <remarks>
	/// Per the specification, only numbers, strings, `null`, `true`,
	/// and `false` are permitted.
	/// </remarks>
	public bool AllowJsonConstructs { get; set; }

	/// <summary>
	/// Gets or sets whether the JSON Path can contain leading and
	/// trailing whitespace.
	/// </summary>
	/// <remarks>
	/// Per the specification, such whitespace is not strictly part of
	/// the syntax, so they are forbidden.  It is expected that such
	/// whitespace will be removed before passing the string onto
	/// parsing functionality.
	/// </remarks>
	[Obsolete("Use TolerateExtraWhitespace instead.")]
	public bool TolerateSurroundingWhitespace { get; set; }

	/// <summary>
	/// Gets or sets whether the JSON Path can contain whitespace in places
	/// the spec says it shouldn't, e.g. between a function name and the
	/// opening parenthesis.
	/// </summary>
	/// <remarks>
	/// Per the specification, leading or trailing whitespace is also
	/// forbidden.  Enabling this option permits this.
	/// </remarks>
	public bool TolerateExtraWhitespace { get; set; }

	/// <summary>
	/// Gets or sets whether the `in` operator is allowed.
	/// </summary>
	/// <remarks>
	/// These operators are an extension of the specification, so they
	/// are disallowed by default.
	///
	/// May also require that <see cref="AllowJsonConstructs"/> be set to
	/// `true` so that array and object literals are also permitted.
	/// </remarks>
	public bool AllowInOperator { get; set; }
}