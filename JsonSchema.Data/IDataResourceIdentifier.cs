using System.Text.Json.Nodes;

namespace Json.Schema.Data;

public interface IDataResourceIdentifier
{
	bool TryResolve(ValidationContext context, out JsonNode? value);
}