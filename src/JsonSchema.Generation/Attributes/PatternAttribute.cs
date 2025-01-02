using System;
using System.Diagnostics.CodeAnalysis;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `pattern` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
	AllowMultiple = true)]
public class PatternAttribute : ConditionalAttribute, INestableAttribute, IAttributeHandler
{
	/// <summary>
	/// The regular expression pattern.
	/// </summary>
	public string Value { get; }

	/// <summary>
	/// The index of the parameter to which the attribute should apply. Default is -1 to indicate the root.
	/// </summary>
	public int GenericParameter { get; set; } = -1;

	/// <summary>
	/// Creates a new <see cref="PatternAttribute"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	public PatternAttribute([StringSyntax(StringSyntaxAttribute.Regex)] string value)
	{
		Value = value;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		if (context.Type != typeof(string)) return;

		context.Intents.Add(new PatternIntent(Value));
	}
}