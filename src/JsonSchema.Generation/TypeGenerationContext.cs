using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Json.Schema.Generation.Intents;

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

	/// <summary>
	/// The number of times this context has been referenced.
	/// </summary>
	public List<MemberGenerationContext> References { get; } = [];

	internal string DefinitionName { get; }

#pragma warning disable IL3050
	internal IComparer<MemberInfo> DeclarationOrderComparer => _memberInfoComparer ??= GetComparer(Type);
#pragma warning restore IL3050

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
			Intents.Add(new RefIntent(uri) { IsExternalRef = true });
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
		foreach (var refiner in refiners.Where(x => x.ShouldRun(this)))
		{
			refiner.Run(this);
		}
	}

	internal bool IsSimpleRef() => Intents is [RefIntent { IsExternalRef: true }];

	[RequiresDynamicCode("JSON serialization and deserialization might require types that cannot be statically analyzed and might need runtime code generation. Use System.Text.Json source generation for native AOT applications.")]
	private static IComparer<MemberInfo> GetComparer(Type type)
	{
		var comparerType = typeof(MemberInfoMetadataTokenComparer<>).MakeGenericType(type);
		var property = comparerType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public);
		var comparer = property!.GetValue(null);

		return (IComparer<MemberInfo>)comparer!;
	}
}