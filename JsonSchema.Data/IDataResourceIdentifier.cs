using System.Text.Json.Nodes;

namespace Json.Schema.Data;

/// <summary>
/// Provides an abstraction for different resource identifier types.
/// </summary>
public interface IDataResourceIdentifier
{
	bool TryResolve(KeywordEvaluation evaluation, SchemaRegistry registry, out JsonNode? value);
}