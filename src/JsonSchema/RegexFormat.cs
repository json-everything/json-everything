﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Json.Schema;

/// <summary>
/// A regular-expression-based format.
/// </summary>
public class RegexFormat : Format
{
	private readonly RegexOrPattern _regex;

	/// <summary>
	/// Creates a new <see cref="RegexFormat"/>.
	/// </summary>
	/// <param name="key">The format key.</param>
	/// <param name="regex">The regular expression.</param>
	public RegexFormat(string key, [StringSyntax(StringSyntaxAttribute.Regex)] string regex)
		: base(key)
	{
		_regex = regex;
	}

	/// <summary>
	/// Creates a new <see cref="RegexFormat"/>.
	/// </summary>
	/// <param name="key">The format key.</param>
	/// <param name="regex">The regular expression.</param>
	public RegexFormat(string key, Regex regex)
		: base(key)
	{
		_regex = regex;
	}

	/// <summary>
	/// Validates an instance against a format and provides an error message.
	/// </summary>
	/// <param name="node">The node to validate.</param>
	/// <param name="errorMessage">An error message.</param>
	/// <returns>`true` if the value is a match for the regular expression; `false` otherwise.</returns>
	public override bool Validate(JsonNode? node, out string? errorMessage)
	{
		if (node.GetSchemaValueType() != SchemaValueType.String)
		{
			errorMessage = null;
			return true;
		}

		var str = node!.GetValue<string>();
		var isMatch =  _regex.IsMatch(str);

		if (!isMatch)
		{
			errorMessage = $"Value is not a match for the format '{Key}'.";
			return false;
		}

		errorMessage = null;
		return isMatch;
	}
}