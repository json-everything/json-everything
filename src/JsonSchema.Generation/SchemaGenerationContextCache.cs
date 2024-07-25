using System;
using System.Collections.Generic;
using Json.Schema.Generation.Intents;

namespace Json.Schema.Generation;

/// <summary>
/// Gets the contexts for the current run.
/// </summary>
public static class SchemaGenerationContextCache
{
	[ThreadStatic]
	private static Dictionary<int, SchemaGenerationContextBase>? _cache;

	internal static Dictionary<int, SchemaGenerationContextBase> Cache => _cache ??= [];

	/// <summary>
	/// Gets or creates a <see cref="SchemaGenerationContextBase"/> based on the given
	/// type and attribute set.
	/// </summary>
	/// <param name="type">The type to generate.</param>
	/// <param name="memberAttributes">
	/// A collection of extra attributes.  Only use if requesting a context to represent
	/// a member.
	/// </param>
	/// <returns>
	/// A generation context, from the cache if one exists with the specified
	/// type and attribute set; otherwise a new one.  New contexts are automatically
	/// cached.  If <paramref name="memberAttributes"/> is null or empty, a
	/// <see cref="TypeGenerationContext"/> will be returned; otherwise a
	/// <see cref="MemberGenerationContext"/>.
	/// </returns>
	/// <remarks>
	/// Use this in your generator if it needs to create keywords with subschemas.
	/// </remarks>
	public static SchemaGenerationContextBase Get(Type type, List<Attribute>? memberAttributes = null)
	{
		return Get(type, memberAttributes, false);
	}

	internal static SchemaGenerationContextBase GetRoot(Type type, List<Attribute>? memberAttributes = null)
	{
		return Get(type, memberAttributes, true);
	}

	private static SchemaGenerationContextBase Get(Type type, List<Attribute>? memberAttributes, bool isRoot)
	{
		var hash = CalculateHash(type, memberAttributes?.WhereHandled());
		if (!Cache.TryGetValue(hash, out var context))
		{
			if (memberAttributes != null && memberAttributes.Count != 0)
			{
				var memberContext = new MemberGenerationContext(type, memberAttributes);
				context = memberContext;
				Cache[hash] = memberContext;
				memberContext.BasedOn = Get(type);
				if (hash != memberContext.BasedOn.Hash)
					memberContext.BasedOn.ReferenceCount--;
			}
			else
			{
				context = new TypeGenerationContext(type) { IsRoot = isRoot };
				var comments = SchemaGeneratorConfiguration.Current.XmlReader.GetTypeComments(type);
				if (!string.IsNullOrWhiteSpace(comments.Summary))
					context.Intents.Add(new DescriptionIntent(comments.Summary!));

				Cache[hash] = context;
			}

			context.Hash = hash;

			context.GenerateIntents();
		}

		context.ReferenceCount++;

		return context;
	}

	internal static void Clear()
	{
		Cache.Clear();
	}

	private static int CalculateHash(Type type, IEnumerable<Attribute>? attributes)
	{
		unchecked
		{
			var hashCode = type.GetHashCode();
			hashCode = (hashCode * 397) ^ (attributes?.GetAttributeSetHashCode() ?? 0);
			return hashCode;
		}
	}
}