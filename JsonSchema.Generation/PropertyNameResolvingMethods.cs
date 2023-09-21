using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Json.Schema.Generation;

/// <summary>
/// Declares a property name resolution which is used to provide a property name.
/// </summary>
/// <param name="input">The property.</param>
/// <returns>The property name or null, if no name was resolved.</returns>
public delegate string? PropertyNameResolvingMethod(MemberInfo input);

/// <summary>
/// Defines a set of predefined property name resolution methods.
/// </summary>
public static class PropertyNameResolvingMethods
{
	/// <summary>
	/// Property name is read from <see cref="JsonPropertyNameAttribute"/>.
	/// </summary>
	public static readonly PropertyNameResolvingMethod ByJsonPropertyName = member => member.GetCustomAttributes<JsonPropertyNameAttribute>().FirstOrDefault()?.Name;

	/// <summary>
	/// Don't do any resolving. Not even <see cref="JsonPropertyNameAttribute"/> will be read.
	/// </summary>
	public static readonly PropertyNameResolvingMethod None = _ => null;
}