using System;
using Json.Schema.Data.Keywords;

namespace Json.Schema.Data;

/// <summary>
/// Provides extension vocabulary definitions for the draft 2020-12 JSON Schema dialect, including support for the data
/// vocabulary.
/// </summary>
/// <remarks>This class exposes constants and dialect configurations related to the draft 2020-12 extension
/// vocabulary. It is intended for use when working with JSON Schema implementations that require the data vocabulary
/// extension. All members are static and thread-safe.</remarks>
public static class Dialect
{
	/// <summary>
	/// The ID for the draft 2020-12 extension vocabulary which includes the data vocabulary.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	public static readonly Uri Data_202012Id = new("https://json-everything.net/meta/data-2023");

	/// <summary>
	/// Provides the JSON Schema dialect definition for the 2020-12 draft with support for the 'data' and 'optionalData'
	/// keywords.
	/// </summary>
	/// <remarks>This dialect enables usage of the 'data' and 'optionalData' keywords as defined in the 2020-12
	/// draft of the JSON Schema specification. Use this dialect when validating schemas that require these keywords. The
	/// dialect is configured with the appropriate meta-schema identifier and keyword support.</remarks>
	// ReSharper disable once InconsistentNaming
	public static readonly Schema.Dialect Data_202012 =
		Schema.Dialect.Draft202012.With([
				DataKeyword.Instance,
				OptionalDataKeyword.Instance
			],
			Data_202012Id,
			false,
			true);
}