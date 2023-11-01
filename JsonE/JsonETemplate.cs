using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Json.More;

namespace Json.JsonE;

[JsonConverter(typeof(JsonETemplateJsonConverter))]
public class JsonETemplate
{
	private readonly IOperator _operator;
	
	internal JsonNode? Template { get; }

	private JsonETemplate(JsonNode? template, IOperator @operator)
	{
		Template = template;
		_operator = @operator;
	}

	public static JsonETemplate Create(JsonNode? node)
	{
		return new JsonETemplate(node, OperatorRepository.Get(node));
	}

	public JsonNode? Evaluate(JsonNode? context)
	{
		var evalContext = new EvaluationContext(context);

		return _operator.Evaluate(Template, evalContext);
	}
}

public class JsonETemplateJsonConverter : JsonConverter<JsonETemplate>
{
	public override JsonETemplate? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = options.Read<JsonNode?>(ref reader);

		return JsonETemplate.Create(node);
	}

	public override void Write(Utf8JsonWriter writer, JsonETemplate value, JsonSerializerOptions options)
	{
		JsonSerializer.Serialize(writer, value.Template, options);
	}
}

internal interface IOperator
{
	JsonNode? Evaluate(JsonNode? template, EvaluationContext context);
}

internal class EvaluationContext
{
	private readonly Stack<JsonNode?> _contextStack;

	public EvaluationContext(JsonNode? baseContext)
	{
		_contextStack = new Stack<JsonNode?>();
		_contextStack.Push(baseContext);
	}

	public void Push(JsonNode? newContext)
	{
		_contextStack.Push(newContext);
	}

	public JsonNode? Pop()
	{
		return _contextStack.Pop();
	}

	public JsonNode? Find(string identifier)
	{
		throw new NotImplementedException();
	}
}

internal class NoOpOperator : IOperator
{
	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		return template;
	}
}

internal class EvalOperator : IOperator
{
	public JsonNode? Evaluate(JsonNode? template, EvaluationContext context)
	{
		throw new NotImplementedException();
	}
}

internal static class OperatorRepository
{
	private static readonly Dictionary<string, IOperator> _operators = new()
	{
		["$eval"] = new EvalOperator(),
	};

	private static readonly IOperator _noOp = new NoOpOperator();

	public static IOperator Get(JsonNode? node)
	{
		if (node is not JsonObject obj) return _noOp;

		var operatorKeys = obj.Select(x => x.Key).Intersect(_operators.Keys).ToArray();
		return operatorKeys.Length switch
		{
			> 1 => throw new TemplateException("only one operator allowed"),
			// TODO: check if `<identifier>` should be replaced by the key
			0 => HasReservedWords(obj) ? throw new TemplateException("TemplateError: $<identifier> is reserved; use $$<identifier>") : _noOp,
			_ => _operators[operatorKeys[0]]
		};
	}

	private static bool HasReservedWords(JsonObject obj)
	{
		return obj.Any(x => Regex.IsMatch(x.Key, @"^\$[^$]"));
	}
}

public class TemplateException : Exception
{
	public TemplateException(string message) : base(message){}
}