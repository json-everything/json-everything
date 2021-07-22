using System.Collections.Generic;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.Tests
{
	public class PropertyNamingTests
	{
		class Target
		{
			public string PropertyThatNeeds_Changing { get; set; }
		}

		public static IEnumerable<TestCaseData> TestCases
		{
			get
			{
				yield return new TestCaseData(PropertyNamingMethods.AsDeclared, "PropertyThatNeeds_Changing");
				yield return new TestCaseData(PropertyNamingMethods.CamelCase, "propertyThatNeedsChanging");
				yield return new TestCaseData(PropertyNamingMethods.PascalCase, "PropertyThatNeedsChanging");
				yield return new TestCaseData(PropertyNamingMethods.KebabCase, "property-that-needs-changing");
				yield return new TestCaseData(PropertyNamingMethods.UpperKebabCase, "PROPERTY-THAT-NEEDS-CHANGING");
				yield return new TestCaseData(PropertyNamingMethods.SnakeCase, "property_that_needs_changing");
				yield return new TestCaseData(PropertyNamingMethods.UpperSnakeCase, "PROPERTY_THAT_NEEDS_CHANGING");
			}
		}

		[TestCaseSource(nameof(TestCases))]
		public void VerifyNameChanges(PropertyNamingMethod namingMethod, string expectedName)
		{
			var config = new SchemaGeneratorConfiguration
			{
				PropertyNamingMethod = namingMethod
			};
			var expected = new JsonSchemaBuilder()
				.Type(SchemaValueType.Object)
				.Properties(
					(expectedName, new JsonSchemaBuilder().Type(SchemaValueType.String))
				)
				.Build();

			var actual = new JsonSchemaBuilder().FromType<Target>(config).Build();

			AssertEqual(expected, actual);
		}
	}
}
