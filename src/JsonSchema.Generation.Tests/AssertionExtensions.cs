using TestHelpers;

// ReSharper disable LocalizableElement

namespace Json.Schema.Generation.Tests;

public static class AssertionExtensions
{
	public static void AssertEqual(JsonSchema expected, JsonSchema actual)
	{
		TestConsole.WriteLine("Expected");
		TestConsole.WriteLine(expected.Root.Source);
		TestConsole.WriteLine();
		TestConsole.WriteLine("Actual");
		TestConsole.WriteLine(actual.Root.Source);
		JsonAssert.AreEquivalent(expected.Root.Source, actual.Root.Source);
	}

	//public static void AssertEqual(PropertiesKeyword expected, PropertiesKeyword actual)
	//{
	//	TestConsole.WriteLine("Expected");
	//	var expectedAsNode = JsonSerializer.SerializeToNode(expected, TestSerializerContext.Default.PropertiesKeyword);
	//	TestConsole.WriteLine(expectedAsNode);
	//	TestConsole.WriteLine();
	//	TestConsole.WriteLine("Actual");
	//	var actualAsNode = JsonSerializer.SerializeToNode(actual, TestSerializerContext.Default.PropertiesKeyword);
	//	TestConsole.WriteLine(actualAsNode);
	//	JsonAssert.AreEquivalent(expectedAsNode, actualAsNode);
	//}

	public static void VerifyGeneration<T>(JsonSchema expected, SchemaGeneratorConfiguration? config = null)
	{
		JsonSchema actual = new JsonSchemaBuilder().FromType<T>(config);
		AssertEqual(expected, actual);
	}
}