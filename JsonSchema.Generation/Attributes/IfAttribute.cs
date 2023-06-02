using System;

namespace Json.Schema.Generation;

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
public class IfAttribute : ConditionalAttribute
{
	public string PropertyName { get; set; }
	public object? Value { get; set; }

	public IfAttribute(string propertyName, object? value, object? group)
	{
		PropertyName = propertyName;
		Value = value;
		ConditionGroup = group;
	}
}