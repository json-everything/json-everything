using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace Json.Schema.Generation.Tests
{
	public class SchemaGenerationTests
	{
		public static IEnumerable<TestCaseData> SimpleTypeCases
		{
			get
			{
				yield return new TestCaseData(typeof(bool), SchemaValueType.Boolean);
				yield return new TestCaseData(typeof(string), SchemaValueType.String);
				yield return new TestCaseData(typeof(byte), SchemaValueType.Integer);
				yield return new TestCaseData(typeof(short), SchemaValueType.Integer);
				yield return new TestCaseData(typeof(ushort), SchemaValueType.Integer);
				yield return new TestCaseData(typeof(int), SchemaValueType.Integer);
				yield return new TestCaseData(typeof(uint), SchemaValueType.Integer);
				yield return new TestCaseData(typeof(long), SchemaValueType.Integer);
				yield return new TestCaseData(typeof(ulong), SchemaValueType.Integer);
				yield return new TestCaseData(typeof(float), SchemaValueType.Number);
				yield return new TestCaseData(typeof(double), SchemaValueType.Number);
				yield return new TestCaseData(typeof(decimal), SchemaValueType.Number);
			}
		}

		[TestCaseSource(nameof(SimpleTypeCases))]
		public void CheckSimpleTypes(Type dotnetType, SchemaValueType schemaType)
		{
			JsonSchema expected = new JsonSchemaBuilder().Type(schemaType);

			JsonSchema actual = new JsonSchemaBuilder().FromType(dotnetType);

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void IntArray()
		{
			JsonSchema expected = new JsonSchemaBuilder().Type(SchemaValueType.Array)
				.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer));

			JsonSchema actual = new JsonSchemaBuilder().FromType<int[]>();

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void ListOfStrings()
		{
			JsonSchema expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Array)
				.Items(new JsonSchemaBuilder().Type(SchemaValueType.String));

			JsonSchema actual = new JsonSchemaBuilder().FromType<List<string>>();

			Assert.AreEqual(expected, actual);
		}

		[Test]
		// ReSharper disable once InconsistentNaming
		public void IEnumerableOfListOfStrings()
		{
			JsonSchema expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Array)
				.Items(new JsonSchemaBuilder()
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder().Type(SchemaValueType.String))
				);

			JsonSchema actual = new JsonSchemaBuilder().FromType<IEnumerable<List<string>>>();

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void GeneralArrayOfThings()
		{
			JsonSchema expected = new JsonSchemaBuilder().Type(SchemaValueType.Array);

			JsonSchema actual = new JsonSchemaBuilder().FromType<Array>();

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void StringDictionaryOfInt()
		{
			JsonSchema expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.AdditionalProperties(new JsonSchemaBuilder().Type(SchemaValueType.Integer));

			JsonSchema actual = new JsonSchemaBuilder().FromType<Dictionary<string, int>>();

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void EnumDictionaryOfInt()
		{
			JsonSchema expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.AdditionalProperties(new JsonSchemaBuilder().Type(SchemaValueType.Integer));

			JsonSchema actual = new JsonSchemaBuilder().FromType<Dictionary<DayOfWeek, int>>();

			Assert.AreEqual(expected, actual);
		}

		[Test]
		public void Enum()
		{
			JsonSchema expected = new JsonSchemaBuilder()
				.Enum(System.Enum.GetNames(typeof(DayOfWeek)).Select(v => v.AsJsonElement()));

			JsonSchema actual = new JsonSchemaBuilder().FromType<DayOfWeek>();

			Assert.AreEqual(expected, actual);
		}

		class GenerationTarget
		{
			[Required]
			[Minimum(5)]
			[ExclusiveMinimum(4)]
			[Maximum(10)]
			[ExclusiveMaximum(11)]
			[MultipleOf(1.5)]
			public int Integer { get; set; }

			[MaxLength(10)]
			[Pattern("^[a-z0-9_]$")]
			public string String { get; set; }

			[MinItems(5)]
			[MaxItems(10)]
			public List<bool> ListOfBool { get; set; }

			[MinLength(5)]
			[UniqueItems(true)]
			[Obsolete]
			public List<string> ListOfString { get; set; }

			[Maximum(100)]
			public int Duplicated1 { get; set; }

			[Maximum(100)]
			public int Duplicated2 { get; set; }

			// TODO: add caching (will need to override the builder extension to take a cache internally)
			// This is going to be interesting given we want to avoid duplication, but duplication
			// isn't just based on type but on the attributes that the property has as well.
		}

		[Test]
		public void GeneratorForObject()
		{
			JsonSchema expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(
					("Integer", new JsonSchemaBuilder()
						.Type(SchemaValueType.Integer)
						.Minimum(5)
						.ExclusiveMinimum(4)
						.Maximum(10)
						.ExclusiveMaximum(11)
						.MultipleOf(1.5m)
					),
					("String", new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.MaxLength(10)
						.Pattern("^[a-z0-9_]$")
					),
					("ListOfBool", new JsonSchemaBuilder()
						.Type(SchemaValueType.Array)
						.Items(new JsonSchemaBuilder().Type(SchemaValueType.Boolean))
						.MinItems(5)
						.MaxItems(10)
					),
					("ListOfString", new JsonSchemaBuilder()
						.Type(SchemaValueType.Array)
						.Items(new JsonSchemaBuilder()
							.Type(SchemaValueType.String)
							.MinLength(5))
						.UniqueItems(true)
						.Deprecated(true)
					),
					("Duplicated1", new JsonSchemaBuilder()
						.Ref("#/$defs/1150770360")
					),
					("Duplicated2", new JsonSchemaBuilder()
						.Ref("#/$defs/1150770360")
					)
				)
				.Required(nameof(GenerationTarget.Integer))
				.Defs(
					("1150770360", new JsonSchemaBuilder()
						.Type(SchemaValueType.Integer)
						.Maximum(100)
					)
				);

			JsonSchema actual = new JsonSchemaBuilder().FromType<GenerationTarget>();

			Console.WriteLine(JsonSerializer.Serialize(actual, new JsonSerializerOptions{WriteIndented = true}));
			Assert.AreEqual(expected, actual);
		}
	}
}