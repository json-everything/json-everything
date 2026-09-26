using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Json.Schema.Api.OpenApi;

/// <summary>
/// Holds properties that are present in the source JSON but are not recognized by this library.
/// </summary>
/// <remarks>
/// Unrecognized properties are captured here rather than rejected, so that a document
/// written against a later version of the specification still loads, and so that
/// serializing it again does not silently discard them.
///
/// This is distinct from <see cref="ExtensionData"/>, which holds only the `x-` prefixed
/// vendor extensions defined by the specification.
/// </remarks>
public class UnknownData : Dictionary<string, JsonNode?>;
