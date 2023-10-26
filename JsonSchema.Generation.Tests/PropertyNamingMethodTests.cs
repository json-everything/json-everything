using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;
// ReSharper disable InconsistentNaming
#pragma warning disable CS0618 // Type or member is obsolete

namespace Json.Schema.Generation.Tests;

public class PropertyNamingMethodTests
{
	private class TargetWithJsonPropertyName
	{
		[JsonPropertyName("JsonName")]
		[UsedImplicitly]
		public string PropertyThatNeeds_Changing { get; set; }
	}

	private class TargetWithoutJsonPropertyName
	{
		[UsedImplicitly]
		public string PropertyThatNeeds_Changing { get; set; }
	}

	public static IEnumerable<TestCaseData> TestCases
	{
		get
		{
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), PropertyNamingMethods.AsDeclared, "PropertyThatNeeds_Changing");
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), PropertyNamingMethods.CamelCase, "propertyThatNeedsChanging");
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), PropertyNamingMethods.PascalCase, "PropertyThatNeedsChanging");
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), PropertyNamingMethods.KebabCase, "property-that-needs-changing");
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), PropertyNamingMethods.UpperKebabCase, "PROPERTY-THAT-NEEDS-CHANGING");
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), PropertyNamingMethods.SnakeCase, "property_that_needs_changing");
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), PropertyNamingMethods.UpperSnakeCase, "PROPERTY_THAT_NEEDS_CHANGING");
			yield return new TestCaseData(typeof(TargetWithoutJsonPropertyName), new PropertyNamingMethod(static _ => "CustomName"), "CustomName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), PropertyNamingMethods.AsDeclared, "JsonName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), PropertyNamingMethods.CamelCase, "JsonName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), PropertyNamingMethods.PascalCase, "JsonName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), PropertyNamingMethods.KebabCase, "JsonName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), PropertyNamingMethods.UpperKebabCase, "JsonName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), PropertyNamingMethods.SnakeCase, "JsonName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), PropertyNamingMethods.UpperSnakeCase, "JsonName");
			yield return new TestCaseData(typeof(TargetWithJsonPropertyName), new PropertyNamingMethod(static _ => "CustomName"), "JsonName");
		}
	}

	[TestCaseSource(nameof(TestCases))]
	public void VerifyNameChanges(Type type, PropertyNamingMethod method, string expectedName)
	{
		var config = new SchemaGeneratorConfiguration
		{
			PropertyNamingMethod = method
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