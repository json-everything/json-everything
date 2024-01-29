using System;
using System.Text.Json;
using NUnit.Framework;
using Json.More;

namespace Json.Schema.Generation.Tests;

internal static class AssertionExtensions
{
	public static void AssertEqual(JsonSchema expected, JsonSchema actual)
	{
		Console.WriteLine("Expected");
		var expectedAsNode = JsonSerializer.SerializeToNode(expected, TestEnvironment.SerializerOptionsWriteIndented);
		Console.WriteLine(expectedAsNode);
		Console.WriteLine();
		Console.WriteLine("Actual");
		var actualAsNode = JsonSerializer.SerializeToNode(actual, TestEnvironment.SerializerOptionsWriteIndented);
		Console.WriteLine(actualAsNode);
		Assert.That(() => actualAsNode.IsEquivalentTo(expectedAsNode));
	}

	public static void AssertEqual(IJsonSchemaKeyword expected, IJsonSchemaKeyword actual)
	{
		Console.WriteLine("Expected");
		var expectedAsNode = JsonSerializer.SerializeToNode(expected, TestEnvironment.SerializerOptionsWriteIndented);
		Console.WriteLine(expectedAsNode);
		Console.WriteLine();
		Console.WriteLine("Actual");
		var actualAsNode = JsonSerializer.SerializeToNode(actual, TestEnvironment.SerializerOptionsWriteIndented);
		Console.WriteLine(actualAsNode);
		Assert.That(() => actualAsNode.IsEquivalentTo(expectedAsNode));
	}

	public static void VerifyGeneration<T>(JsonSchema expected, SchemaGeneratorConfiguration? config = null)
	{
		JsonSchema actual = new JsonSchemaBuilder().FromType<T>(config);
		AssertEqual(expected, actual);
	}
}