using System;

namespace Json.Schema.Generation;

public abstract class ConditionalAttribute : Attribute
{
	public object? ConditionGroup { get; set; }
}