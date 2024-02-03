using System;
using System.Text.Json;
using NUnit.Framework;
using Json.More;
// ReSharper disable LocalizableElement

namespace Json.Schema.Generation.Tests;

internal static class AssertionExtensions
{
	public static void AssertEqual(JsonSchema expected, JsonSchema actual)
	{
		Console.WriteLine("Expected");
		var expectedAsNode = JsonSerializer.SerializeToNode(expected, TestSerializerContext.Default.JsonSchema);
		Console.WriteLine(expectedAsNode);
		Console.WriteLine();
		Console.WriteLine("Actual");
		var actualAsNode = JsonSerializer.SerializeToNode(actual, TestSerializerContext.Default.JsonSchema);
		Console.WriteLine(actualAsNode);
		Assert.That(() => actualAsNode.IsEquivalentTo(expectedAsNode));
	}

	public static void AssertEqual(PropertiesKeyword expected, PropertiesKeyword actual)
	{
		Console.WriteLine("Expected");
		var expectedAsNode = JsonSerializer.SerializeToNode(expected, TestSerializerContext.Default.PropertiesKeyword);
		Console.WriteLine(expectedAsNode);
		Console.WriteLine();
		Console.WriteLine("Actual");
		var actualAsNode = JsonSerializer.SerializeToNode(actual, TestSerializerContext.Default.PropertiesKeyword);
		Console.WriteLine(actualAsNode);
		Assert.That(() => actualAsNode.IsEquivalentTo(expectedAsNode));
	}

	public static void VerifyGeneration<T>(JsonSchema expected, SchemaGeneratorConfiguration? config = null)
	{
		JsonSchema actual = new JsonSchemaBuilder().FromType<T>(config);
		AssertEqual(expected, actual);
	}
}