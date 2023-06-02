using System;

namespace Json.Schema.Generation;

/// <summary>
/// Serves as a base class for attributes which support conditional schema generation.
/// </summary>
public abstract class ConditionalAttribute : Attribute
{
	/// <summary>
	/// Identifies the condition group under which this attribute applies.
	/// </summary>
	public object? ConditionGroup { get; set; }
}