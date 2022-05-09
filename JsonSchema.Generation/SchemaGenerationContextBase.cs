using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Json.Schema.Generation.Refiners;

namespace Json.Schema.Generation;

public abstract class SchemaGenerationContextBase
{
	private IComparer<MemberInfo>? _memberInfoComparer;

	public Type Type { get; protected set; }

	public int ReferenceCount { get; set; }

	public List<ISchemaKeywordIntent> Intents { get; } = new();

	internal IComparer<MemberInfo> DeclarationOrderComparer => _memberInfoComparer ??= GetComparer(Type);

	internal int Hash { get; set; }

	/// <summary>
	/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
	/// </summary>
	/// <param name="builder">The schema builder.</param>
	/// <returns>The schema builder (for fluent syntax support).</returns>
	public JsonSchemaBuilder Apply(JsonSchemaBuilder? builder = null)
	{
		builder ??= new JsonSchemaBuilder();

		foreach (var intent in Intents)
		{
			intent.Apply(builder);
		}

		return builder;
	}

	internal void GenerateIntents()
	{
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
		var comparer = property.GetValue(null);

		return (IComparer<MemberInfo>)comparer;
	}
}