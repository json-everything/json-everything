using System.Text.Json;
using Json.Schema.CodeGeneration.Language;

namespace Json.Schema.CodeGeneration.Tests;

public static class AssertHelpers
{
	public static string VerifyCSharp(JsonSchema schema, string expected, EvaluationOptions? options = null)
	{
		var code = schema.GenerateCode(CodeWriters.CSharp, options);

		Console.WriteLine(code);
		Assert.AreEqual(expected, code);

		return code;
	}

	public static void VerifyDeserialization(string code, string json)
	{
		var assembly = Compiler.Compile(code);
		Assert.NotNull(assembly, "Could not compile assembly");

		var targetType = assembly!.DefinedTypes.First();
		var model = JsonSerializer.Deserialize(json, targetType);
		Assert.NotNull(model);
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