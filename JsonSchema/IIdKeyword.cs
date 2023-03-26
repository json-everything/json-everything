using System;

namespace Json.Schema;

/// <summary>
/// Allows definition of a custom schema identifier keyword.
/// WARNING - Foot-gun mode: use only if you know what you're doing.
/// </summary>
public interface IIdKeyword : IJsonSchemaKeyword
{
	/// <summary>
	/// Defines the URI ID.
	/// </summary>
	public Uri Id { get; }
}