using System;
using JetBrains.Annotations;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `pattern` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class PatternAttribute : Attribute, IAttributeHandler
{
	/// <summary>
	/// The regular expression pattern.
	/// </summary>
	public string Value { get; }

	/// <summary>
	/// Creates a new <see cref="PatternAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public PatternAttribute([RegexPattern] string value)
	{
		Value = value;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (context.Type != typeof(string)) return;

		context.Intents.Add(new PatternIntent(Value));
	}
}