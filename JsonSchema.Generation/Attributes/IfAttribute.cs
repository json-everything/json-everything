using System;

namespace Json.Schema.Generation;

/// <summary>
/// Creates or amends a condition group by expecting a value in a property.
/// </summary>
[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
public class IfAttribute : ConditionalAttribute
{
	/// <summary>
	/// The property name.
	/// </summary>
	public string PropertyName { get; set; }
	/// <summary>
	/// The expected property value.
	/// </summary>
	/// <remarks>
	/// The compiler will allow any compile-time constant, however only JSON-compatible
	/// values will work.
	/// </remarks>
	public object? Value { get; set; }

	/// <summary>
	/// Creates a new <see cref="IfAttribute"/> instance.
	/// </summary>
	/// <param name="propertyName">The name of the property.</param>
	/// <param name="value">The expected value for the property (should be JSON-compatible).</param>
	/// <param name="group">The condition group.</param>
	public IfAttribute(string propertyName, object? value, object? group)
	{
		PropertyName = propertyName;
		Value = value;
		ConditionGroup = group;
	}
}