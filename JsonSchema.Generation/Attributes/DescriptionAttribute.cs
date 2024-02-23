using System;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `description` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface,
	AllowMultiple = true)]
public class DescriptionAttribute : ConditionalAttribute, IAttributeHandler
{
	/// <summary>
	/// The description.
	/// </summary>
	public string Description { get; }

	/// <summary>
	/// Creates a new <see cref="DescriptionAttribute"/> instance.
	/// </summary>
	/// <param name="description">The value.</param>
	public DescriptionAttribute(string description)
	{
		Description = description;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		var existingDescription = context.Intents.Where(x => x is DescriptionIntent).ToArray();
		foreach (var intent in existingDescription)
		{
			context.Intents.Remove(intent);
		}

		context.Intents.Add(new DescriptionIntent(Description));
	}
}