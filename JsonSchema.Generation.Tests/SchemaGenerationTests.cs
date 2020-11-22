using System;
using System.Collections.Generic;
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
	}
}