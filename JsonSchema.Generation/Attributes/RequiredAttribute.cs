using System;

namespace Json.Schema.Generation;

/// <summary>
/// Indicates a property is required and should be listed in the
/// `required` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class RequiredAttribute : ConditionalAttribute
{
	internal string PropertyName { get; set; }
}