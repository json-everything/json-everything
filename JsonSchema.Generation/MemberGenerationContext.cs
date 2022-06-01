using System;
using System.Collections.Generic;

namespace Json.Schema.Generation;

/// <summary>
/// Provides context for object members to include those attributes.
/// </summary>
public class MemberGenerationContext : SchemaGenerationContextBase
{
	/// <summary>
	/// Gets the context this is based on.
	/// </summary>
	public SchemaGenerationContextBase BasedOn { get; }

	/// <summary>
	/// Gets the set of member attributes.
	/// </summary>
	public List<Attribute> Attributes { get; }

	internal MemberGenerationContext(SchemaGenerationContextBase basedOn, List<Attribute> attributes)
		: base(basedOn.Type)
	{
		BasedOn = basedOn;
		Attributes = attributes;

		if (Hash != BasedOn.Hash)
			BasedOn.ReferenceCount--;
	}
}