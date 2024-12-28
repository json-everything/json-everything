using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Json.Schema.Generation.Intents;
using Json.Schema.Generation.Refiners;

namespace Json.Schema.Generation;

/// <summary>
/// Provides a context for generating schemas for types.
/// </summary>
[DebuggerDisplay("{DebuggerDisplay}")]
public class TypeGenerationContext : SchemaGenerationContextBase
{
	private IComparer<MemberInfo>? _memberInfoComparer;

	/// <summary>
	/// The type.
	/// </summary>
	public override Type Type { get; }

	internal string DefinitionName { get; }

	internal IComparer<MemberInfo> DeclarationOrderComparer => _memberInfoComparer ??= GetComparer(Type);

	internal TypeGenerationContext(Type type)
	{
		Type = type;
		DebuggerDisplay = type.CSharpName();
		DefinitionName = type.GetDefName();
	}

	internal override void GenerateIntents()
	{
		if (ReferenceEquals(this, True) || ReferenceEquals(this, False)) return;

		var configuration = SchemaGeneratorConfiguration.Current;

		if (configuration.ExternalReferences.TryGetValue(Type, out var uri))
			Intents.Add(new RefIntent(uri));
		else
		{
			var runGenerator = true;
			if (!IsRoot)
			{
				var idAttribute = Type.GetCustomAttributes<IdAttribute>().SingleOrDefault();
				if (idAttribute is not null)
				{
					Intents.Add(new RefIntent(idAttribute.Uri));
					runGenerator = false;
				}
			}

			if (runGenerator)
			{
				var generator = configuration.Generators.FirstOrDefault(x => x.Handles(Type)) ?? GeneratorRegistry.Get(Type);
				generator?.AddConstraints(this);
			}
		}

		AttributeHandler.HandleAttributes(this);

		var refiners = configuration.Refiners.ToList();
		refiners.Add(NullabilityRefiner.Instance);
		foreach (var refiner in refiners.Where(x => x.ShouldRun(this)))
		{
			refiner.Run(this);
		}
	}

	private static IComparer<MemberInfo> GetComparer(Type type)
	{
		var comparerType = typeof(MemberInfoMetadataTokenComparer<>).MakeGenericType(type);
		var property = comparerType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
		var comparer = property!.GetValue(null);

		return (IComparer<MemberInfo>)comparer!;
	}
}