#if NETSTANDARD2_0

using System;
using System.Text.Json;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Determines how <see cref="JsonSerializer"/> handles numbers when serializing and deserializing.
	/// </summary>
	[Flags]
	public enum JsonNumberHandling
	{
		/// <summary>
		/// Numbers will only be read from <see cref="JsonTokenType.Number"/> tokens and will only be written as JSON numbers (without quotes).
		/// </summary>
		Strict = 0x0,

		/// <summary>
		/// Numbers can be read from <see cref="JsonTokenType.String"/> tokens.
		/// Does not prevent numbers from being read from <see cref="JsonTokenType.Number"/> token.
		/// </summary>
		AllowReadingFromString = 0x1,

		/// <summary>
		/// Numbers will be written as JSON strings (with quotes), not as JSON numbers.
		/// </summary>
		WriteAsString = 0x2,

		/// <summary>
		/// The "NaN", "Infinity", and "-Infinity" <see cref="JsonTokenType.String"/> tokens can be read as
		/// floating-point constants, and the <see cref="float"/> and <see cref="double"/> values for these
		/// constants will be written as their corresponding JSON string representations.
		/// </summary>
		AllowNamedFloatingPointLiterals = 0x4
	}
}

#endif