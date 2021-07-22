using System;
using System.Text.Json.Serialization;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Indicates that the property should be excluded from generation.
	/// </summary>
	/// <remarks>
	/// This attribute functions exactly the same as the <see cref="JsonIgnoreAttribute"/>.  It
	/// is included separately to support the case where the model should be serialized with
	/// a property but schema generation should ignore it.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class JsonExcludeAttribute : JsonAttribute
	{
		/// <summary>
		/// Initializes a new instance of <see cref="JsonExcludeAttribute"/>.
		/// </summary>
		// ReSharper disable once EmptyConstructor (it's not redundant if it defines new comments ;)
		public JsonExcludeAttribute() { }
	}
}