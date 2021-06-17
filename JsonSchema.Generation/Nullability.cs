using System;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Indicates whether to apply `null` to schema type.
	/// </summary>
	/// <remarks>
	/// This is a flags-enabled enumeration, so values that support multiple
	/// types can be bit-wise OR'd `|` together.
	/// </remarks>
	[Flags]
	public enum Nullability
	{
		/// <summary>
		/// Default value. Type `null` will not be applied to schema.
		/// </summary>
		Disabled = 1 << 0,
		/// <summary>
		/// Type `null` will be applied to Nullable&lt;T&gt; schema.
		/// </summary>
		AllowForNullableValueTypes = 1 << 1,
		/// <summary>
		/// Type `null` will be applied to Reference Type schema
		/// </summary>
		AllowForReferenceTypes = 1 << 2,
		/// <summary>
		/// Type `null` will be applied to both Reference Type and Nullable&lt;T&gt; schema
		/// </summary>
		AllowForAllTypes = AllowForNullableValueTypes | AllowForReferenceTypes,

	}
}