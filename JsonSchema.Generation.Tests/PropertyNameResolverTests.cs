using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Json.Schema.Generation.Tests;

public class PropertyNameResolverTests
{
	private class TargetWithJsonPropertyName
	{
		[JsonPropertyName("JsonName")]
		public string PropertyThatNeeds_Changing { get; set; }
	}

	private class TargetWithoutJsonPropertyName
	{
		public string PropertyThatNeeds_Changing { get; set; }
	}

	public static IEnumerable<TestCaseData> TestCases
	{
		get
		{
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), PropertyNameResolvers.AsDeclared, "PropertyThatNeeds_Changing");
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), PropertyNameResolvers.CamelCase, "propertyThatNeedsChanging");
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), PropertyNameResolvers.PascalCase, "PropertyThatNeedsChanging");
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), PropertyNameResolvers.KebabCase, "property-that-needs-changing");
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), PropertyNameResolvers.UpperKebabCase, "PROPERTY-THAT-NEEDS-CHANGING");
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), PropertyNameResolvers.SnakeCase, "property_that_needs_changing");
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), PropertyNameResolvers.UpperSnakeCase, "PROPERTY_THAT_NEEDS_CHANGING");
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), new PropertyNameResolver(static _ => "CustomName"), "CustomName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), PropertyNameResolvers.AsDeclared, "JsonName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), PropertyNameResolvers.CamelCase, "JsonName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), PropertyNameResolvers.PascalCase, "JsonName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), PropertyNameResolvers.KebabCase, "JsonName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), PropertyNameResolvers.UpperKebabCase, "JsonName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), PropertyNameResolvers.SnakeCase, "JsonName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), PropertyNameResolvers.UpperSnakeCase, "JsonName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), new PropertyNameResolver(static _ => "CustomName"), "JsonName");
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