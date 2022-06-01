using System;
using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation;

/// <summary>
/// Gets the contexts for the current run.
/// </summary>
public static class SchemaGenerationContextCache
{
	[ThreadStatic]
	private static Dictionary<int, SchemaGenerationContextBase>? _cache;

	internal static Dictionary<int, SchemaGenerationContextBase> Cache => _cache ??= new();

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
		var hash = CalculateHash(type, memberAttributes?.WhereHandled());
		if (!Cache.TryGetValue(hash, out var context))
		{
			if (memberAttributes != null && memberAttributes.Any())
			{
				var basedOn = Get(type);
				context = new MemberGenerationContext(basedOn, memberAttributes);
			}
			else
				context = new TypeGenerationContext(type);

			context.Hash = hash;
			Cache[hash] = context;

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