using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Provides context for object members to include those attributes.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay}")]
public class MemberGenerationContext : SchemaGenerationContextBase
{
	/// <summary>
	/// The type.
	/// </summary>
	public override Type Type => BasedOn.Type;

	/// <summary>
	/// Gets the context this is based on.
	/// </summary>
	public TypeGenerationContext BasedOn { get; }

	/// <summary>
	/// Gets the set of member attributes.
	/// </summary>
	public List<Attribute> Attributes { get; }

	/// <summary>
	/// Indicates whether the member is marked as a nullable reference type.
	/// </summary>
	public bool NullableRef { get; }

	/// <summary>
	/// Gets or sets the generic parameter that this context represents.
	/// A null value (default) represents the root type.
	/// </summary>
	public int Parameter { get; set; } = -1;

	internal MemberGenerationContext(TypeGenerationContext basedOn, List<Attribute> attributes, bool markedAsNullableRef = false)
	{
		BasedOn = basedOn;
		Attributes = attributes;
		NullableRef = markedAsNullableRef;

		DebuggerDisplay = BasedOn.Type.CSharpName() + $" + [{string.Join(",", attributes.Select(x => x.GetType().CSharpName().Replace("Attribute", string.Empty)))}]";
	}

	internal MemberGenerationContext(MemberGenerationContext source)
	{
		BasedOn = source.BasedOn;
		Attributes = [..source.Attributes];
		Intents.AddRange(source.Intents);

		DebuggerDisplay = BasedOn.Type.CSharpName() + $"[{string.Join(",", Attributes.Select(x => x.GetType().CSharpName().Replace("Attribute", string.Empty)))}]";
	}

	internal sealed override void GenerateIntents()
	{
		if (ReferenceEquals(this, True) || ReferenceEquals(this, False)) return;

		var nullableAttribute = Attributes.OfType<NullableAttribute>().FirstOrDefault();
		var nullable = nullableAttribute?.IsNullable ?? NullableRef;
		List<ISchemaKeywordIntent> baseIntents;
		if (BasedOn.IsRoot)
			baseIntents = [new RefIntent(this, new Uri("#", UriKind.Relative))];
		else if (nullable || !Type.CanBeReferenced() || BasedOn.IsSimpleRef())
			baseIntents = BasedOn.Intents;
		else
		{
			baseIntents = [new RefIntent(this, new Uri($"#/$defs/{BasedOn.DefinitionName}", UriKind.Relative))];
			BasedOn.References.Add(this);
		}

		Intents.AddRange(nullable ? baseIntents.AsNullable() : baseIntents);

		AttributeHandler.HandleAttributes(this);

		var refiners = SchemaGeneratorConfiguration.Current.Refiners.ToList();
		foreach (var refiner in refiners.Where(x => x.ShouldRun(this)))
		{
			refiner.Run(this);
		}
	}
}