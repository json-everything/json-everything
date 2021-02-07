#if NETSTANDARD2_0

using System;
using System.Text.Json.Serialization;

namespace Json.Schema.Generation
{
	/// <summary>
	/// When placed on a type, property, or field, indicates what <see cref="JsonNumberHandling"/>
	/// settings should be used when serializing or deserializing numbers.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class JsonNumberHandlingAttribute : JsonAttribute
	{
		/// <summary>
		/// Indicates what settings should be used when serializing or deserializing numbers.
		/// </summary>
		public JsonNumberHandling Handling { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="JsonNumberHandlingAttribute"/>.
		/// </summary>
		public JsonNumberHandlingAttribute(JsonNumberHandling handling)
		{
			var handlingValue = (int) handling;
			if (handlingValue < 0 || 7 < handlingValue)
				throw new ArgumentOutOfRangeException(nameof(handling));

			Handling = handling;
		}
	}
}

#endif
