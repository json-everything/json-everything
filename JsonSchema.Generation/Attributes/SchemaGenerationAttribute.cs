using System;

namespace Json.Schema.Generation;

public abstract class SchemaGenerationAttribute : Attribute
{
	public object? ConditionGroup { get; set; }
}