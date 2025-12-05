using System;
using Json.Schema.ArrayExt.Keywords;

namespace Json.Schema.ArrayExt;

/// <summary>
/// Provides predefined JSON Schema dialects and extension vocabulary identifiers for use with Draft 2020-12 schemas,
/// including support for array-specific extensions.
/// </summary>
/// <remarks>Use the members of this class to reference dialects and vocabulary IDs when working with JSON Schema
/// validation that requires array ordering or unique key features. The dialects defined here extend the standard Draft
/// 2020-12 specification to support additional array-related keywords.</remarks>
public static class Dialect
{
	/// <summary>
	/// The ID for the draft 2020-12 extension vocabulary which includes the array extensions vocabulary.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static readonly Uri ArrayExt_202012Id = new("https://json-everything.net/meta/array-ext");

	/// <summary>
	/// Represents the JSON Schema dialect for Draft 2020-12 with array extension keywords added.
	/// </summary>
	/// <remarks>This dialect includes support for array ordering and unique keys via the OrderingKeyword and
	/// UniqueKeysKeyword extensions. Use this dialect when validating schemas that require these array-specific features
	/// in accordance with the 2020-12 draft specification.</remarks>
	// ReSharper disable once InconsistentNaming
	public static readonly Schema.Dialect ArrayExt_202012 =
		Schema.Dialect.Draft202012.With([
				OrderingKeyword.Instance,
				UniqueKeysKeyword.Instance
			],
			ArrayExt_202012Id,
			false,
			true);
}