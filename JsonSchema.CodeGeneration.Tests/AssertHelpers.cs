using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using Json.More;
using Json.Schema.CodeGeneration.Language;

namespace Json.Schema.CodeGeneration.Tests;

public static class AssertHelpers
{
	private static readonly JsonSerializerOptions _options =
		new()
		{
			TypeInfoResolverChain = { JsonSchema.TypeInfoResolver }
		};

	private static readonly JsonSerializerOptions _optionsWithReflection =
		new(_options)
		{
			TypeInfoResolverChain = { new DefaultJsonTypeInfoResolver() }
		};

	private static readonly JsonSerializerOptions _optionsUnsafeRelaxedJsonEscaping =
		new(_options)
		{
			WriteIndented = true,
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};

	public static string VerifyCSharp(JsonSchema schema, string expected, EvaluationOptions? options = null)
	{
		var code = schema.GenerateCode(CodeWriters.CSharp, options);

		Console.WriteLine(code);
		Assert.AreEqual(expected, code);

		return code;
	}

	public static void VerifyDeserialization(string code, string json, bool isReflectionAllowed = false)
	{
		var assembly = Compiler.Compile(code);
		Assert.NotNull(assembly, "Could not compile assembly");

		var targetType = assembly!.DefinedTypes.First();
		var model = JsonSerializer.Deserialize(json, targetType, isReflectionAllowed ? _optionsWithReflection : _options);
		Assert.NotNull(model);

		var node = JsonNode.Parse(json);
		var returnToNode = JsonSerializer.SerializeToNode(model, isReflectionAllowed ? _optionsWithReflection : _options);

		Console.WriteLine(returnToNode.AsJsonString(_optionsUnsafeRelaxedJsonEscaping));
		Assert.True(node.IsEquivalentTo(returnToNode));
	}

	public static void VerifyFailure(JsonSchema schema, EvaluationOptions? options = null)
	{
		var ex = Assert.Throws<UnsupportedSchemaException>(() =>
		{
			var actual = schema.GenerateCode(CodeWriters.CSharp, options);

			Console.WriteLine(actual);
		});

		Console.WriteLine(ex);
	}
}