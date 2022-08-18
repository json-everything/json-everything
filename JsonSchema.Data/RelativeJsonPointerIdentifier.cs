using System.Text.Json.Nodes;
using Json.Pointer;

namespace Json.Schema.Data;

public class RelativeJsonPointerIdentifier : IDataResourceIdentifier
{
	public RelativeJsonPointer Target { get; }

	public RelativeJsonPointerIdentifier(RelativeJsonPointer target)
	{
		Target = target;
	}

	public bool TryResolve(ValidationContext context, out JsonNode? value)
	{
		return Target.TryEvaluate(context.LocalInstance, out value);
	}

	public override string ToString()
	{
		return Target.ToString();
	}
}