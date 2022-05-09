using System;
using System.Collections.Generic;

namespace Json.Schema.Generation;

public class MemberGenerationContext : SchemaGenerationContextBase
{
	public SchemaGenerationContextBase BasedOn { get; }

	public List<Attribute> Attributes { get; }

	internal MemberGenerationContext(SchemaGenerationContextBase basedOn, List<Attribute> attributes)
	{
		BasedOn = basedOn;
		Attributes = attributes;
		Type = basedOn.Type;
	}
}