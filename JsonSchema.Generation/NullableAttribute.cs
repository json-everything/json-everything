using System;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Overrides the <see cref="SchemaGeneratorConfiguration.Nullability"/> option and either
	/// adds or removes `null` in the `type` keyword.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property)]
	public class NullableAttribute : Attribute
	{
		/// <summary>
		/// Gets whether `null` should be included in the `type` keyword.
		/// </summary>
		public bool IsNullable { get; }

		/// <summary>
		/// Creates a new <see cref="NullableAttribute"/> instance.
		/// </summary>
		/// <param name="isNullable">Whether `null` should be included in the `type` keyword.</param>
		public NullableAttribute(bool isNullable)
		{
			IsNullable = isNullable;
		}
	}
}