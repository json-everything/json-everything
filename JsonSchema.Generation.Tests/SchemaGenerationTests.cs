using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using static Json.Schema.Generation.Tests.AssertionExtensions;

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
				yield return new TestCaseData(typeof(bool?), SchemaValueType.Boolean);
				yield return new TestCaseData(typeof(byte?), SchemaValueType.Integer);
				yield return new TestCaseData(typeof(short?), SchemaValueType.Integer);
				yield return new TestCaseData(typeof(ushort?), SchemaValueType.Integer);
				yield return new TestCaseData(typeof(int?), SchemaValueType.Integer);
				yield return new TestCaseData(typeof(uint?), SchemaValueType.Integer);
				yield return new TestCaseData(typeof(long?), SchemaValueType.Integer);
				yield return new TestCaseData(typeof(ulong?), SchemaValueType.Integer);
				yield return new TestCaseData(typeof(float?), SchemaValueType.Number);
				yield return new TestCaseData(typeof(double?), SchemaValueType.Number);
				yield return new TestCaseData(typeof(decimal?), SchemaValueType.Number);
			}
		}

		[TestCaseSource(nameof(SimpleTypeCases))]
		public void CheckSimpleTypes(Type dotnetType, SchemaValueType schemaType)
		{
			JsonSchema expected = new JsonSchemaBuilder().Type(schemaType);

			JsonSchema actual = new JsonSchemaBuilder().FromType(dotnetType);

			AssertEqual(expected, actual);
		}

		[Test]
		public void IntArray()
		{
			JsonSchema expected = new JsonSchemaBuilder().Type(SchemaValueType.Array)
				.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer));

			JsonSchema actual = new JsonSchemaBuilder().FromType<int[]>();

			AssertEqual(expected, actual);
		}

		[Test]
		public void NullableIntArray()
		{
			JsonSchema expected = new JsonSchemaBuilder().Type(SchemaValueType.Array)
				.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer));

			JsonSchema actual = new JsonSchemaBuilder().FromType<int?[]>();

			AssertEqual(expected, actual);
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

			AssertEqual(expected, actual);
		}

		[Test]
		public void GeneralArrayOfThings()
		{
			JsonSchema expected = new JsonSchemaBuilder().Type(SchemaValueType.Array);

			JsonSchema actual = new JsonSchemaBuilder().FromType<Array>();

			AssertEqual(expected, actual);
		}

		[Test]
		public void StringDictionaryOfInt()
		{
			JsonSchema expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.AdditionalProperties(new JsonSchemaBuilder().Type(SchemaValueType.Integer));

			JsonSchema actual = new JsonSchemaBuilder().FromType<Dictionary<string, int>>();

			AssertEqual(expected, actual);
		}

		[Test]
		public void EmptyObject()
		{
			JsonSchema expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object);

			JsonSchema actual = new JsonSchemaBuilder().FromType<object>();

			AssertEqual(expected, actual);
		}

		// ReSharper disable once ClassNeverInstantiated.Local
		private class GenerationTarget
		{
			[JsonInclude]
#pragma warning disable 169
			private int _value;

			private int _notIncluded;
#pragma warning restore 169

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

			[Required]
			public string RequiredString { get; set; }

			[JsonPropertyName("rename-this-required-string")]
			[Required]
			public string RenameThisRequiredString { get; set; }

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

			public GenerationTarget Target { get; set; }

			[JsonIgnore]
			public int IgnoreThis { get; set; }

			[JsonPropertyName("rename-this")]
			public string RenameThis { get; set; }

			public float StrictNumber { get; set; }
			public float OtherStrictNumber { get; set; }

			[ReadOnly]
			public float ReadOnlyNumber { get; set; }

			[WriteOnly]
			public float WriteOnlyNumber { get; set; }

			[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
			public float StringyNumber { get; set; }

			[JsonNumberHandling(JsonNumberHandling.AllowNamedFloatingPointLiterals)]
			public float NotANumber { get; set; }

			[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.AllowNamedFloatingPointLiterals)]
			public float StringyNotANumber { get; set; }

			[Title("title")]
			[Description("description")]
			public string Metadata { get; set; }
		}

		[Test]
		public void GeneratorForObject()
		{
			JsonSchema expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(
					("_value", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
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
					("RequiredString", new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
					),
					("rename-this-required-string", new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
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
						.Ref("#/$defs/integer")
					),
					("Duplicated2", new JsonSchemaBuilder()
						.Ref("#/$defs/integer")
					),
					("Target", JsonSchemaBuilder.RefRoot()),
					("rename-this", new JsonSchemaBuilder().Type(SchemaValueType.String)),
					("StrictNumber", new JsonSchemaBuilder().Type(SchemaValueType.Number)),
					("OtherStrictNumber", new JsonSchemaBuilder().Type(SchemaValueType.Number)),
					("ReadOnlyNumber", new JsonSchemaBuilder()
						.Type(SchemaValueType.Number)
						.ReadOnly(true)
					),
					("WriteOnlyNumber", new JsonSchemaBuilder()
						.Type(SchemaValueType.Number)
						.WriteOnly(true)
					),
					("StringyNumber", new JsonSchemaBuilder().Type(SchemaValueType.String | SchemaValueType.Number)),
					("NotANumber", new JsonSchemaBuilder()
						.AnyOf(new JsonSchemaBuilder().Type(SchemaValueType.Number),
							new JsonSchemaBuilder().Enum("NaN", "Infinity", "-Infinity")
						)
					),
					("StringyNotANumber", new JsonSchemaBuilder()
						.AnyOf(new JsonSchemaBuilder().Type(SchemaValueType.String | SchemaValueType.Number),
							new JsonSchemaBuilder().Enum("NaN", "Infinity", "-Infinity")
						)
					),
					("Metadata", new JsonSchemaBuilder()
						.Type(SchemaValueType.String)
						.Title("title")
						.Description("description")
					)
				)
				.Required(nameof(GenerationTarget.Integer), "RequiredString", "rename-this-required-string")
				.Defs(
					("integer", new JsonSchemaBuilder()
						.Type(SchemaValueType.Integer)
						.Maximum(100)
					)
				);

			JsonSchema actual = new JsonSchemaBuilder().FromType<GenerationTarget>();

			AssertEqual(expected, actual);
		}
	}
}