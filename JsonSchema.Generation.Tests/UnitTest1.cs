using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Json.Schema.Generation.Tests
{
	public class SchemaGenerationTests
	{
		public static IEnumerable<TestCaseData> Cases
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

		[TestCaseSource(nameof(Cases))]
		public void CheckSimpleTypes(Type dotnetType, SchemaValueType schemaType)
		{
			JsonSchema expected = new JsonSchemaBuilder().Type(schemaType);

			JsonSchema actual = new JsonSchemaBuilder().FromType(dotnetType);

			Assert.AreEqual(expected, actual);
		}
	}
}