using System;

namespace Json.Schema.Generation;

[AttributeUsage(AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
	AllowMultiple = true)]
public class IfEnumAttribute : SchemaGenerationAttribute, IAttributeHandler<IfAttribute>
{
	public string PropertyName { get; set; }

	public IfEnumAttribute(string propertyName)
	{
		PropertyName = propertyName;
	}

	public void AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		
	}
}