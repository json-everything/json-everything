using System;

namespace Json.Schema.Generation;

/// <summary>
/// Creates multiple condition groups based on the value of an enum property, one group for each defined enum value.
/// </summary>
/// <remarks>
/// The enum type is inferred from the property.
/// </remarks>
[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
	AllowMultiple = true)]
public class IfEnumAttribute : ConditionalAttribute, IConditionAttribute
{
	/// <summary>
	/// The property name.
	/// </summary>
	public string PropertyName { get; set; }
	/// <summary>
	/// Gets or sets whether to use numbers or names in the condition.  Default is to use names.
	/// </summary>
	public bool UseNumbers { get; set; }

	/// <summary>
	/// Creates a new <see cref="IfEnumAttribute"/> instance.
	/// </summary>
	/// <param name="propertyName">The property name.</param>
	/// <param name="useNumbers">(optional) Whether to use numbers or names in the condition.  Default is to use names.</param>
	public IfEnumAttribute(string propertyName, bool useNumbers = false)
	{
		PropertyName = propertyName;
		UseNumbers = useNumbers;
	}
}