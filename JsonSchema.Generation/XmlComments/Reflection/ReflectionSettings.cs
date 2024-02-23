using System;
using System.Reflection;

namespace Json.Schema.Generation.XmlComments.Reflection;

/// <summary>
///     Settings used by TypeCollection to retrieve reflection info.
/// </summary>
public class ReflectionSettings
{
	/// <summary>
	///     Default reflection settings.
	/// </summary>
	public static ReflectionSettings Default => new()
	{
		PropertyFlags =
			BindingFlags.Instance |
			BindingFlags.Public |
			BindingFlags.Static,

		MethodFlags =
			BindingFlags.DeclaredOnly |
			BindingFlags.Instance |
			BindingFlags.Public |
			BindingFlags.Static,

		FieldFlags =
			BindingFlags.DeclaredOnly |
			BindingFlags.Instance |
			BindingFlags.Public |
			BindingFlags.Static,

		NestedTypeFlags =
			BindingFlags.DeclaredOnly |
			BindingFlags.Instance |
			BindingFlags.Public |
			BindingFlags.Static
	};

	/// <summary>
	///     Binding flags to use when retrieving properties of the type.
	/// </summary>
	public BindingFlags PropertyFlags { get; set; }

	/// <summary>
	///     Binding flags to use when retrieving methods of the type.
	/// </summary>
	public BindingFlags MethodFlags { get; set; }

	/// <summary>
	///     Binding flags to use when retrieving fields of the type.
	/// </summary>
	public BindingFlags FieldFlags { get; set; }

	/// <summary>
	///     Binding flags to use when retrieving nested types of the type.
	/// </summary>
	public BindingFlags NestedTypeFlags { get; set; }

	/// <summary>
	///     Checks if specified property should be added to the list of properties and the
	///     set of referenced types.
	///     Return true if property and types referenced by it should be examined.
	///     Function should return false if property should be ignored.
	///     Default implementation returns true for all properties.
	/// </summary>
	public Func<PropertyInfo, bool>? PropertyFilter { get; set; }

	/// <summary>
	///     Checks if specified method should be added to the list of methods and the
	///     set of referenced types.
	///     Return true if the method and types referenced by it should be examined.
	///     Function should return false if method should be ignored.
	///     Default implementation returns true for all methods.
	/// </summary>
	public Func<MethodBase, bool>? MethodFilter { get; set; }

	/// <summary>
	///     Checks if specified field should be added to the list of fields and the
	///     set of referenced types.
	///     Return true if field and types referenced by it should be examined.
	///     Function should return false if field should be ignored.
	///     Default implementation returns true for all fields.
	/// </summary>
	public Func<FieldInfo, bool>? FieldFilter { get; set; }
}
