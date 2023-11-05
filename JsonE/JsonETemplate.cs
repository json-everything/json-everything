using System.Text.Json;
using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
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
		if (node is JsonValue value && value.GetValue<object>() is JsonETemplate template)
			return template;

		var (op, newTemplate) = OperatorRepository.Get(node);
		return new JsonETemplate(newTemplate, op);
	}

	internal static JsonNode? CreateNode(JsonNode? node)
	{
		if (node is JsonValue value && value.GetValue<object>() is JsonETemplate template)
			return template;

		var (op, newTemplate) = OperatorRepository.Get(node);
		return op is null ? node : new JsonETemplate(newTemplate, op);
	}

	/// <summary>
	/// Evaluates the template against a JSON value context.
	/// </summary>
	/// <param name="context">The JSON value context</param>
	/// <returns>A new JSON value result.</returns>
	public JsonNode? Evaluate(JsonNode? context)
	{
		ValidateContext(context);

		var evalContext = new EvaluationContext(context);

		return Evaluate(evalContext);
	}

	private static void ValidateContext(JsonNode? context)
	{
		if (context is not JsonObject obj)
			throw new TemplateException("context must be an object");

		if (obj.Any(x => !Regex.IsMatch(x.Key, "^[a-zA-Z_][a-zA-Z0-9_]*$")))
			throw new TemplateException("top level keys of context must follow /[a-zA-Z_][a-zA-Z0-9_]*/");
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

internal class JsonETemplateJsonConverter : JsonConverter<JsonETemplate?>
{
	public override JsonETemplate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = options.Read<JsonNode?>(ref reader);

		return JsonETemplate.Create(node);
	}

	public override void Write(Utf8JsonWriter writer, JsonETemplate? value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value?.Template, options);
	}
}