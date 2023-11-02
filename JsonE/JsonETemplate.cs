using System.Text.Json;
using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.JsonE;

/// <summary>
/// Models a JSON-e template.
/// </summary>
[JsonConverter(typeof(JsonETemplateJsonConverter))]
public class JsonETemplate
{
	private readonly IOperator? _operator;
	
	internal JsonNode? Template { get; }

	private JsonETemplate(JsonNode? template, IOperator? @operator)
	{
		Template = template;
		_operator = @operator;
	}

	/// <summary>
	/// Creates a new template.
	/// </summary>
	/// <param name="node">The JSON representation of the template.</param>
	/// <returns></returns>
	public static JsonETemplate Create(JsonNode? node)
	{
		return CreateInternal(node.Copy());
	}

	internal static JsonETemplate CreateInternal(JsonNode? node)
	{
		if (node is JsonValue value && value.GetValue<object>() is JsonETemplate template)
			return template;

		var (op, newTemplate) = OperatorRepository.Get(node);
		return new JsonETemplate(newTemplate, op);
	}

	/// <summary>
	/// Evaluates the template against a JSON value context.
	/// </summary>
	/// <param name="context">The JSON value context</param>
	/// <returns>A new JSON value result.</returns>
	public JsonNode? Evaluate(JsonNode? context)
	{
		var evalContext = new EvaluationContext(context);

		return Evaluate(evalContext);
	}

	internal JsonNode? Evaluate(EvaluationContext context)
	{
		return _operator?.Evaluate(Template, context) ?? Template;
	}

	//public static implicit operator JsonETemplate(JsonNode? node)
	//{
	//	return Create(node);
	//}

	/// <summary>
	/// Implicitly wraps a <see cref="JsonETemplate"/> in a <see cref="JsonValue"/>.
	/// </summary>
	/// <param name="template"></param>
	public static implicit operator JsonNode(JsonETemplate template)
	{
		return JsonValue.Create(template)!;
	}
}

internal class JsonETemplateJsonConverter : JsonConverter<JsonETemplate>
{
	public override JsonETemplate? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = options.Read<JsonNode?>(ref reader);

		return JsonETemplate.CreateInternal(node);
	}

	public override void Write(Utf8JsonWriter writer, JsonETemplate value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Template, options);
	}
}