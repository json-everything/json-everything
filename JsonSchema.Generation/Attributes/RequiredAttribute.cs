using System;

namespace Json.Schema.Generation;

/// <summary>
/// Indicates a property is required and should be listed in the
/// `required` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class RequiredAttribute : ConditionalAttribute
{
#pragma warning disable CS8618
	internal string PropertyName { get; set; }
#pragma warning restore CS8618
}