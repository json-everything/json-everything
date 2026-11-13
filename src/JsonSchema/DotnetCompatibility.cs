using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Json.Schema;

/// <summary>
/// Provides compatibility extension methods that replicate .NET API behaviors on earlier framework versions.
/// </summary>
/// <remarks>This class contains static extension methods that offer functionality introduced in later .NET
/// versions, enabling codebases to use newer APIs when targeting older frameworks. Methods are conditionally included
/// based on the target framework version and are intended to ease cross-version development and migration.</remarks>
public static class DotnetCompatibility
{
#if !NET8_0_OR_GREATER

	/// <summary>
	/// Retrieves the value associated with the specified key from the dictionary, or the default value for the type if the
	/// key is not found.
	/// </summary>
	/// <remarks>This method provides a convenient way to retrieve a value from a dictionary without throwing an
	/// exception if the key does not exist. If <paramref name="dictionary"/> is null, a <see
	/// cref="ArgumentNullException"/> will be thrown.</remarks>
	/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
	/// <param name="dictionary">The read-only dictionary to search for the specified key. Cannot be null.</param>
	/// <param name="key">The key whose value to retrieve.</param>
	/// <returns>The value associated with the specified key if the key is found; otherwise, the default value for the type
	/// <typeparamref name="TValue"/>.</returns>
	public static TValue? GetValueOrDefault<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
	{
		return dictionary.TryGetValue(key, out var value) ? value : default;
	}

	/// <summary>
	/// Attempts to add the specified key and value to the dictionary if the key does not already exist.
	/// </summary>
	/// <remarks>This method does not overwrite an existing value if the key is already present in the dictionary.
	/// If the key exists, the method returns false and does not modify the dictionary.</remarks>
	/// <typeparam name="TKey">The type of keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
	/// <param name="dictionary">The dictionary to which the key and value will be added. Cannot be null.</param>
	/// <param name="key">The key to add to the dictionary. Cannot be null if the dictionary implementation does not allow null keys.</param>
	/// <param name="value">The value to associate with the specified key.</param>
	/// <returns>true if the key and value were added to the dictionary; otherwise, false.</returns>
	public static bool TryAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
	{
		if (dictionary.ContainsKey(key)) return false;

		dictionary.Add(key, value);
		return true;
	}

#endif

#if !NET9_0_OR_GREATER

	/// <summary>
	/// Returns the number of properties contained in the specified JSON object.
	/// </summary>
	/// <remarks>If <paramref name="element"/> does not represent a JSON object, the result will be 0. This method
	/// does not count properties in nested objects.</remarks>
	/// <param name="element">The <see cref="System.Text.Json.JsonElement"/> to count properties for. Must represent a JSON object.</param>
	/// <returns>The number of properties in the JSON object. Returns 0 if the object contains no properties.</returns>
	public static int GetPropertyCount(this JsonElement element) => element.EnumerateObject().Count();

#endif

	/// <summary>
	/// Performs a conversion from <see cref="Double"/> to <see cref="Decimal"/> while optionally allowing
	/// for .Net 11's breaking change.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="useNet10CompatibleRounding">(.Net 11 only) <c>>true</c> forces .Net 10 and earlier rounding from <c>>double</c> to <c>>decimal</c></param>
	public static decimal ConvertToDecimal(this double value, bool useNet10CompatibleRounding = false)
	{
#if NET11_0_OR_GREATER
		var converted = (decimal)value;
		if (useNet10CompatibleRounding && converted != 0)
		{
			// .Net 10 and earlier kept 15 significant digits when converting double to decimal.
			var scale = 14 - (int)Math.Floor(Math.Log10(Math.Abs(value)));
			if (scale < 0)
			{
				var factor = (decimal)Math.Pow(10, -scale);
				converted = Math.Round(converted / factor) * factor;
			}
			else if (scale <= 28)
				converted = Math.Round(converted, scale);

			// Dividing by one at maximum scale strips the trailing zeros left by rounding.
			converted /= 1.0000000000000000000000000000m;
		}

		return converted;
#else
		return (decimal)value;
#endif
	}
}
