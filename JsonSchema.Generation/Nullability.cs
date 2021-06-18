using System;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Indicates whether to apply `null` to the `type` keyword.
	/// </summary>
	[Flags]
	public enum Nullability
	{
		/// <summary>
		/// Indicates that `null` will not be included.
		/// </summary>
		Disabled = 0,
		/// <summary>
		/// Indicates that `null` will be applied to <see cref="Nullable{T}"/> types.
		/// </summary>
		AllowForNullableValueTypes = 1 << 0,
		/// <summary>
		/// Indicates that `null` will be applied to reference types.
		/// </summary>
		AllowForReferenceTypes = 1 << 1,
		/// <summary>
		/// Indicates that `null` will be applied to both reference types and <see cref="Nullable{T}"/> types.
		/// </summary>
		AllowForAllTypes = AllowForNullableValueTypes | AllowForReferenceTypes
	}
}