using System;
using System.Text.Json;
using Json.Schema.Generation.Intents;
using NUnit.Framework;

namespace Json.Schema.Generation.Tests
{
	public class RefinerTests
	{
		private class TwoProps
		{
			public int Value1 { get; set; }
			public string Value2 { get; set; }
		}

		private class ThreeProps
		{
			public int Value1 { get; set; }
			public string Value2 { get; set; }
			public bool Value3 { get; set; }
		}

		private class Refiner : ISchemaRefiner
		{
			public bool ShouldRun(SchemaGeneratorContext context)
			{
				return context.Type.GetProperties().Length % 2 == 1;
			}

			public void Run(SchemaGeneratorContext context)
			{
				context.Intents.Add(new ReadOnlyIntent(true));
			}
		}

		[Test]
		public void TypesWithAnOddNumberOfPropertiesShouldBeImmutable()
		{
			var configuration = new SchemaGeneratorConfiguration
			{
				Refiners = {new Refiner()}
			};

			JsonSchema expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(
					("Value1", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
					("Value2", new JsonSchemaBuilder().Type(SchemaValueType.String)),
					("Value3", new JsonSchemaBuilder().Type(SchemaValueType.Boolean))
				)
				.ReadOnly(true);

			JsonSchema actual = new JsonSchemaBuilder().FromType<ThreeProps>(configuration);

			Console.WriteLine(JsonSerializer.Serialize(expected, new JsonSerializerOptions {WriteIndented = true}));
			Console.WriteLine(JsonSerializer.Serialize(actual, new JsonSerializerOptions {WriteIndented = true}));
			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void TypesWithAnEvenNumberOfPropertiesShouldBeMutable()
		{
			var configuration = new SchemaGeneratorConfiguration
			{
				Refiners = {new Refiner()}
			};

			JsonSchema expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(
					("Value1", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
					("Value2", new JsonSchemaBuilder().Type(SchemaValueType.String))
				);

			JsonSchema actual = new JsonSchemaBuilder().FromType<TwoProps>(configuration);

			Console.WriteLine(JsonSerializer.Serialize(expected, new JsonSerializerOptions {WriteIndented = true}));
			Console.WriteLine(JsonSerializer.Serialize(actual, new JsonSerializerOptions {WriteIndented = true}));
			Assert.AreEqual(expected, actual);
		}
	}
}