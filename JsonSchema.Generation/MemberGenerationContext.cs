using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Json.Schema.Generation;

/// <summary>
/// Provides context for object members to include those attributes.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay}")]
public class MemberGenerationContext : SchemaGenerationContextBase
{
	/// <summary>
	/// Gets the context this is based on.
	/// </summary>
	public SchemaGenerationContextBase BasedOn { get; internal set; }

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

		DebuggerDisplay = Type.CSharpName() + $"[{string.Join(",", attributes.Select(x => x.GetType().CSharpName().Replace("Attribute", string.Empty)))}]";
	}

	internal MemberGenerationContext(Type basedOnType, List<Attribute> attributes)
		: base(basedOnType)
	{
		Attributes = attributes;

		DebuggerDisplay = Type.CSharpName() + $"[{string.Join(",", attributes.Select(x => x.GetType().CSharpName().Replace("Attribute", string.Empty)))}]";
	}
}