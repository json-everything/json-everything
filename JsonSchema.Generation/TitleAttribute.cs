using System;
using System.Collections.Generic;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Applies a `title` keyword.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field |
				AttributeTargets.Enum | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class TitleAttribute : Attribute, IAttributeHandler
{
	/// <summary>
	/// The title.
	/// </summary>
	public string Title { get; }

	/// <summary>
	/// Creates a new <see cref="TitleAttribute"/> instance.
	/// </summary>
	/// <param name="title">The value.</param>
	public TitleAttribute(string title)
	{
		Title = title;
	}

	void IAttributeHandler.AddConstraints(SchemaGenerationContextBase context, Attribute attribute)
	{
		context.Intents.Add(new TitleIntent(Title));
	}
}