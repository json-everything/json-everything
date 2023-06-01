using System;

namespace Json.Schema.Generation;

[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
public abstract class SchemaGenerationAttribute : Attribute
{
	public object? ConditionGroup { get; set; }
}