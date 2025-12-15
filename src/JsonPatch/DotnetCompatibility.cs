using System.Collections.Generic;

namespace Json.Patch;

/// <summary>
/// Provides compatibility extension methods that replicate .NET API behaviors on earlier framework versions.
/// </summary>
/// <remarks>
/// This class contains static extension methods that offer functionality introduced in later .NET
/// versions, enabling codebases to use newer APIs when targeting older frameworks. Methods are conditionally included
/// based on the target framework version and are intended to ease cross-version development and migration.
/// </remarks>
internal static class DotnetCompatibility
{
#if NETSTANDARD2_0
	/// <summary>
	/// Deconstructs a generic KeyValuePair into its Key and Value properties.
	/// </summary>
	/// <remarks>
	/// This is useful for iterating over a generic dictionary collection.
	/// <example><code>foreach (var (key, value) in myDictionary) { ... }</code></example>
	/// https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.keyvaluepair-2.deconstruct
	/// </remarks>
	public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
	{
		key = kvp.Key;
		value = kvp.Value;
	}
#endif
}
