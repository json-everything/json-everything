using System;
using System.Text.Json.Nodes;

namespace Json.Schema.Generation;

/// <summary>
/// Creates or amends a condition group by expecting a value in a property.
/// </summary>
[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
public class IfAttribute : ConditionalAttribute, INestableAttribute, IConditionalAttribute
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
	public JsonNode? Value { get; set; }

	/// <summary>
	/// The index of the parameter to which the attribute should apply. Default is -1 to indicate the root.
	/// </summary>
	public int GenericParameter { get; set; } = -1;

	/// <summary>
	/// Creates a new <see cref="IfAttribute"/> instance.
	/// </summary>
	/// <param name="propertyName">The name of the property.</param>
	/// <param name="value">The expected value for the property.</param>
	/// <param name="group">The condition group.</param>
	public IfAttribute(string propertyName, int value, object? group)
	{
		PropertyName = propertyName;
		Value = value;
		ConditionGroup = group;
	}

	/// <summary>
	/// Creates a new <see cref="IfAttribute"/> instance.
	/// </summary>
	/// <param name="propertyName">The name of the property.</param>
	/// <param name="value">The expected value for the property.</param>
	/// <param name="group">The condition group.</param>
	public IfAttribute(string propertyName, uint value, object? group)
	{
		PropertyName = propertyName;
		Value = value;
		ConditionGroup = group;
	}

	/// <summary>
	/// Creates a new <see cref="IfAttribute"/> instance.
	/// </summary>
	/// <param name="propertyName">The name of the property.</param>
	/// <param name="value">The expected value for the property.</param>
	/// <param name="group">The condition group.</param>
	public IfAttribute(string propertyName, long value, object? group)
	{
		PropertyName = propertyName;
		Value = value;
		ConditionGroup = group;
	}

	/// <summary>
	/// Creates a new <see cref="IfAttribute"/> instance.
	/// </summary>
	/// <param name="propertyName">The name of the property.</param>
	/// <param name="value">The expected value for the property.</param>
	/// <param name="group">The condition group.</param>
	public IfAttribute(string propertyName, ulong value, object? group)
	{
		PropertyName = propertyName;
		Value = value;
		ConditionGroup = group;
	}

	/// <summary>
	/// Creates a new <see cref="IfAttribute"/> instance.
	/// </summary>
	/// <param name="propertyName">The name of the property.</param>
	/// <param name="value">The expected value for the property.</param>
	/// <param name="group">The condition group.</param>
	public IfAttribute(string propertyName, float value, object? group)
	{
		PropertyName = propertyName;
		Value = value;
		ConditionGroup = group;
	}

	/// <summary>
	/// Creates a new <see cref="IfAttribute"/> instance.
	/// </summary>
	/// <param name="propertyName">The name of the property.</param>
	/// <param name="value">The expected value for the property.</param>
	/// <param name="group">The condition group.</param>
	public IfAttribute(string propertyName, double value, object? group)
	{
		PropertyName = propertyName;
		Value = value;
		ConditionGroup = group;
	}

	/// <summary>
	/// Creates a new <see cref="IfAttribute"/> instance.
	/// </summary>
	/// <param name="propertyName">The name of the property.</param>
	/// <param name="value">The expected value for the property.</param>
	/// <param name="group">The condition group.</param>
	public IfAttribute(string propertyName, bool value, object? group)
	{
		PropertyName = propertyName;
		Value = value;
		ConditionGroup = group;
	}

	/// <summary>
	/// Creates a new <see cref="IfAttribute"/> instance.
	/// </summary>
	/// <param name="propertyName">The name of the property.</param>
	/// <param name="value">The expected value for the property.</param>
	/// <param name="group">The condition group.</param>
	public IfAttribute(string propertyName, string? value, object? group)
	{
		PropertyName = propertyName;
		Value = value;
		ConditionGroup = group;
	}
}