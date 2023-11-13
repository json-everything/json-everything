using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;

namespace Json.JsonE.Operators;

internal class LetOperator : IOperator
{
	public const string Name = "$let";
	
	public void Validate(JsonNode? template)
	{
		var obj = template!.AsObject();

		var parameter = obj[Name];
		if (parameter.IsTemplateOr<JsonObject>()) return;

		obj.VerifyNoUndefinedProperties(Name, "in");

		if (obj.Count != 2)
			throw new TemplateException($"{Name} requires `in` property");
	}

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();
		var value = obj[Name];

		var additionalContext = JsonE.Evaluate(value, context);
		additionalContext.ValidateAsContext(Name);

		context.Push(additionalContext);
		var result = JsonE.Evaluate(obj["in"], context);
		context.Pop();

		return result;
	}
}

internal class JsonOperator : IOperator
{
	private static readonly JsonSerializerOptions _serializerOptions =
		new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

	public const string Name = "$json";
	
	public void Validate(JsonNode? template)
	{
		var obj = template!.AsObject();

		obj.VerifyNoUndefinedProperties(Name);
	}

	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		var obj = template!.AsObject();
		var value = obj[Name];

		var evaluated = JsonE.Evaluate(value, context);

		return evaluated.AsJsonString(_serializerOptions);
	}
}