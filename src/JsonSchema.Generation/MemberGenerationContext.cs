using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Json.Schema.Generation.Intents;
using Json.Schema.Generation.Refiners;

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
	public SchemaGenerationContextBase BasedOn { get; }

	/// <summary>
	/// Gets the set of member attributes.
	/// </summary>
	public List<Attribute> Attributes { get; }

	internal MemberGenerationContext(SchemaGenerationContextBase basedOn, List<Attribute> attributes)
	{
		BasedOn = basedOn;
		Attributes = attributes;

		DebuggerDisplay = BasedOn.Type.CSharpName() + $"[{string.Join(",", attributes.Select(x => x.GetType().CSharpName().Replace("Attribute", string.Empty)))}]";

		GenerateIntents();
	}

	internal sealed override void GenerateIntents()
	{
		if (ReferenceEquals(this, True) || ReferenceEquals(this, False)) return;

		var configuration = SchemaGeneratorConfiguration.Current;

		Intents.Add(new RefIntent(new Uri($"#/$defs/{BasedOn.Type.FullName}", UriKind.Relative)));

		AttributeHandler.HandleAttributes(this);

		var refiners = configuration.Refiners.ToList();
		refiners.Add(NullabilityRefiner.Instance);
		foreach (var refiner in refiners.Where(x => x.ShouldRun(this)))
		{
			refiner.Run(this);
		}
	}
}