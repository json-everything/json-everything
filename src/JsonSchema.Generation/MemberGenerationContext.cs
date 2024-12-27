using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Json.Schema.Generation.Refiners;
using Json.Schema.Generation.Intents;

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

		DebuggerDisplay = Type.CSharpName() + $"[{string.Join(",", attributes.Select(x => x.GetType().CSharpName().Replace("Attribute", string.Empty)))}]";

		GenerateIntents();
	}

#pragma warning disable CS8618
	internal MemberGenerationContext(Type basedOnType, List<Attribute> attributes)
		: base(basedOnType)
	{
		Attributes = attributes;

		DebuggerDisplay = Type.CSharpName() + $"[{string.Join(",", attributes.Select(x => x.GetType().CSharpName().Replace("Attribute", string.Empty)))}]";

		GenerateIntents();
	}
#pragma warning restore CS8618

	internal override void GenerateIntents()
	{
		if (ReferenceEquals(this, True) || ReferenceEquals(this, False)) return;

		var configuration = SchemaGeneratorConfiguration.Current;

		var runGenerator = true;

		if (runGenerator)
		{
			var generator = configuration.Generators.FirstOrDefault(x => x.Handles(Type)) ?? GeneratorRegistry.Get(Type);
			generator?.AddConstraints(this);
		}

		AttributeHandler.HandleAttributes(this);

		var refiners = configuration.Refiners.ToList();
		refiners.Add(NullabilityRefiner.Instance);
		foreach (var refiner in refiners.Where(x => x.ShouldRun(this)))
		{
			refiner.Run(this);
		}
	}
}