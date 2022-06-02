using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Json.Schema.Generation.Refiners;

namespace Json.Schema.Generation;

/// <summary>
/// Provides base functionality and data for generation contexts.
/// </summary>
public abstract class SchemaGenerationContextBase
{
	internal class TrueType { }

	internal class FalseType { }

	private IComparer<MemberInfo>? _memberInfoComparer;

	/// <summary>
	/// Represents a true schema.
	/// </summary>
	public static readonly SchemaGenerationContextBase True = new TypeGenerationContext(typeof(TrueType));
	/// <summary>
	/// Represents a false schema.
	/// </summary>
	public static readonly SchemaGenerationContextBase False = new TypeGenerationContext(typeof(FalseType));

	/// <summary>
	/// The type.
	/// </summary>
	public Type Type { get; }

	/// <summary>
	/// The number of times this context has been referenced.
	/// </summary>
	public int ReferenceCount { get; set; }

	/// <summary>
	/// The keyword intents required for this type.
	/// </summary>
	public List<ISchemaKeywordIntent> Intents { get; } = new();

	/// <summary>
	/// A calculated hash value that represents and identifies this context.
	/// </summary>
	public int Hash { get; set; }

	internal IComparer<MemberInfo> DeclarationOrderComparer => _memberInfoComparer ??= GetComparer(Type);

	/// <summary>
	/// Creates a new context.
	/// </summary>
	/// <param name="type">The type represented by the context.</param>
	protected SchemaGenerationContextBase(Type type)
	{
		Type = type;
	}

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The schema builder.</param>
	/// <returns>The schema builder (for fluent syntax support).</returns>
	public JsonSchema Apply(JsonSchemaBuilder? builder = null)
	{
		if (ReferenceEquals(this, True)) return true;
		if (ReferenceEquals(this, False)) return false;

		if (!Intents.Any()) return true;

		builder ??= new JsonSchemaBuilder();

		foreach (var intent in Intents)
		{
			intent.Apply(builder);
		}

		return builder;
	}

	internal void GenerateIntents()
	{
		if (ReferenceEquals(this, True) || ReferenceEquals(this, False)) return;

		var configuration = SchemaGeneratorConfiguration.Current;

		var generator = configuration.Generators.FirstOrDefault(x => x.Handles(Type)) ?? GeneratorRegistry.Get(Type);
		generator?.AddConstraints(this);

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