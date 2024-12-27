using System;
using System.Collections.Generic;
using System.Linq;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Gets the contexts for the current run.
/// </summary>
public static class SchemaGenerationContextCache
{
	[ThreadStatic]
	private static Dictionary<Type, TypeGenerationContext>? _cache;

	internal static Dictionary<Type, TypeGenerationContext> Cache => _cache ??= [];

	/// <summary>
	/// Gets or creates a <see cref="TypeGenerationContext"/> based on the given
	/// type and attribute set.
	/// </summary>
	/// <param name="type">The type to generate.</param>
	/// <returns>
	/// A generation context, from the cache if one exists with the specified
	/// type and attribute set; otherwise a new one.  New contexts are automatically
	/// cached.
	/// </returns>
	/// <remarks>
	/// Use this in your generator if it needs to create keywords with subschemas.
	/// </remarks>
	public static TypeGenerationContext Get(Type type)
	{
		return Get(type, false);
	}

	internal static TypeGenerationContext GetRoot(Type type)
	{
		var baseContext = Get(type, true);

		var definitions = Cache.Where(x => x.Key != type && !x.Key.IsKnownType())
			.ToDictionary(x => x.Key.FullName!, SchemaGenerationContextBase (x) => x.Value);

		if (definitions.Count != 0)
			baseContext.Intents.Add(new DefsIntent(definitions));

		return baseContext;
	}

	private static TypeGenerationContext Get(Type type, bool isRoot)
	{
		if (!Cache.TryGetValue(type, out var context))
		{
			context = new TypeGenerationContext(type) { IsRoot = isRoot };
			var comments = SchemaGeneratorConfiguration.Current.XmlReader.GetTypeComments(type);
			if (!string.IsNullOrWhiteSpace(comments.Summary))
				context.Intents.Add(new DescriptionIntent(comments.Summary!));

			Cache[type] = context;

			context.GenerateIntents();
		}

		context.ReferenceCount++;

		return context;
	}

	internal static void Clear()
	{
		Cache.Clear();
	}
}