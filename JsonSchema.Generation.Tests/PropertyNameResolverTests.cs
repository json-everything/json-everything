using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.Tests;

public class PropertyNameResolverTests
{
	class Target
	{
		[JsonPropertyName("JsonName")]
		public string PropertyThatNeeds_Changing { get; set; }
	}

	class TargetWithOutJsonPropertyName
	{
		public string PropertyThatNeeds_Changing { get; set; }
	}

	public static IEnumerable<TestCaseData> TestCases
	{
		get
		{
			yield return new TestCaseData(typeof(Target), PropertyNameResolvers.AsDeclared, "PropertyThatNeeds_Changing");
			yield return new TestCaseData(typeof(Target), PropertyNameResolvers.CamelCase, "propertyThatNeedsChanging");
			yield return new TestCaseData(typeof(Target), PropertyNameResolvers.PascalCase, "PropertyThatNeedsChanging");
			yield return new TestCaseData(typeof(Target), PropertyNameResolvers.KebabCase, "property-that-needs-changing");
			yield return new TestCaseData(typeof(Target), PropertyNameResolvers.UpperKebabCase, "PROPERTY-THAT-NEEDS-CHANGING");
			yield return new TestCaseData(typeof(Target), PropertyNameResolvers.SnakeCase, "property_that_needs_changing");
			yield return new TestCaseData(typeof(Target), PropertyNameResolvers.UpperSnakeCase, "PROPERTY_THAT_NEEDS_CHANGING");
			yield return new TestCaseData(typeof(Target), PropertyNameResolvers.ByJsonPropertyName, "JsonName");
			yield return new TestCaseData(typeof(Target), new PropertyNameResolver(static x => "CustomName"), "CustomName");
			yield return new TestCaseData(typeof(TargetWithOutJsonPropertyName), PropertyNameResolvers.ByJsonPropertyName, "PropertyThatNeeds_Changing");
		}
	}

	[TestCaseSource(nameof(TestCases))]
	public void VerifyNameChanges(Type type, PropertyNameResolver resolver, string expectedName)
	{
		var config = new SchemaGeneratorConfiguration
		{
			PropertyNameResolver = resolver
		};
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				(expectedName, new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Build();

		var actual = new JsonSchemaBuilder().FromType(type, config).Build();

		AssertEqual(expected, actual);
	}
}