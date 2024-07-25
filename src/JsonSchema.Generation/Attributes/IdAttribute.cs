using System;
using System.Diagnostics.CodeAnalysis;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies an `$id` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class IdAttribute : Attribute, IAttributeHandler
{
	/// <summary>
	/// The regular expression pattern.
	/// </summary>
	public Uri Uri { get; }

	/// <summary>
	/// Creates a new <see cref="IdAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public IdAttribute([StringSyntax(StringSyntaxAttribute.Regex)] string value)
	{
		if (!Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute))
			throw new ArgumentException("[Id] attribute requires a valid URI", nameof(value));

		Uri = new Uri(value, UriKind.RelativeOrAbsolute);
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		context.Intents.Insert(0, new IdIntent(Uri));
	}
}