using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema.Data;

public class JsonPointerIdentifier : IDataResourceIdentifier
{
	public JsonPointer Target { get; }

	public JsonPointerIdentifier(JsonPointer target)
	{
		Target = target;
	}

	public bool TryResolve(ValidationContext context, out JsonNode? value)
	{
		return Target.TryEvaluate(context.InstanceRoot, out value);
	}

	public override string ToString()
	{
		return Target.ToString();
	}
}