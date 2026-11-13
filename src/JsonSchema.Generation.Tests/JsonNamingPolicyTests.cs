#if NET11_0_OR_GREATER
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;
// ReSharper disable UnusedMember.Local
// ReSharper disable ClassNeverInstantiated.Local

namespace Json.Schema.Generation.Tests;

public class JsonNamingPolicyTests
{
	[JsonNamingPolicy(JsonKnownNamingPolicy.SnakeCaseUpper)]
	private class TypePolicy
	{
		public string FirstValue { get; set; }
		[JsonNamingPolicy(JsonKnownNamingPolicy.KebabCaseLower)]
		public string SecondValue { get; set; }
		[JsonPropertyName("Third")]
		public string ThirdValue { get; set; }
		[JsonNamingPolicy(JsonKnownNamingPolicy.KebabCaseLower)]
		[JsonPropertyName("Fourth")]
		public string FourthValue { get; set; }
	}

	private class MemberPolicy
	{
		public string FirstValue { get; set; }
		[JsonNamingPolicy(JsonKnownNamingPolicy.SnakeCaseLower)]
		public string SecondValue { get; set; }
	}

	private class DerivedFromTypePolicy : TypePolicy
	{
		public string FifthValue { get; set; }
	}

	private class ShoutingPolicy : JsonNamingPolicy
	{
		public override string ConvertName(string name) => name.ToUpperInvariant();
	}

	private class ShoutingAttribute : JsonNamingPolicyAttribute
	{
		public ShoutingAttribute() : base(new ShoutingPolicy())
		{
		}
	}

	[Shouting]
	private class CustomPolicy
	{
		public string FirstValue { get; set; }
	}

	[JsonNamingPolicy(JsonKnownNamingPolicy.SnakeCaseLower)]
	[If(nameof(TriggerValue), "go", "group")]
	private class ConditionalPolicy
	{
		public string TriggerValue { get; set; }
		[MinLength(5, ConditionGroup = "group")]
		public string AffectedValue { get; set; }
	}

	[Test]
	public void TypePolicyOverridesConfiguration()
	{
		var config = new SchemaGeneratorConfiguration
		{
			PropertyNameResolver = PropertyNameResolvers.CamelCase
		};
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("FIRST_VALUE", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("second-value", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("Third", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("Fourth", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Build();

		var actual = new JsonSchemaBuilder().FromType<TypePolicy>(config).Build();

		AssertEqual(expected, actual);
	}

	[Test]
	public void MemberPolicyOverridesConfiguration()
	{
		var config = new SchemaGeneratorConfiguration
		{
			PropertyNameResolver = PropertyNameResolvers.CamelCase
		};
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("firstValue", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("second_value", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Build();

		var actual = new JsonSchemaBuilder().FromType<MemberPolicy>(config).Build();

		AssertEqual(expected, actual);
	}

	[Test]
	public void TypePolicyIsNotInherited()
	{
		var config = new SchemaGeneratorConfiguration
		{
			PropertyNameResolver = PropertyNameResolvers.CamelCase,
			PropertyOrder = PropertyOrder.ByName
		};
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("fifthValue", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("firstValue", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("Fourth", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("second-value", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("Third", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Build();

		var actual = new JsonSchemaBuilder().FromType<DerivedFromTypePolicy>(config).Build();

		AssertEqual(expected, actual);
	}

	[Test]
	public void CustomPolicyAttributeIsUsed()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("FIRSTVALUE", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Build();

		var actual = new JsonSchemaBuilder().FromType<CustomPolicy>().Build();

		AssertEqual(expected, actual);
	}

	[Test]
	public void PolicyAppliesToConditionals()
	{
		var expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("trigger_value", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("affected_value", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.If(new JsonSchemaBuilder()
				.Properties(
					("trigger_value", new JsonSchemaBuilder().Const("go"))
				)
				.Required("trigger_value")
			)
			.Then(new JsonSchemaBuilder()
				.Properties(
					("affected_value", new JsonSchemaBuilder().Type(SchemaValueType.String).MinLength(5))
				)
			)
			.Build();

		var actual = new JsonSchemaBuilder().FromType<ConditionalPolicy>().Build();

		AssertEqual(expected, actual);
	}
}
#endif
