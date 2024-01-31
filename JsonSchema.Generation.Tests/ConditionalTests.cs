using System;
using System.Collections.Generic;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;
// ReSharper disable UnusedMember.Global
// ReSharper disable ClassNeverInstantiated.Local
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Json.Schema.Generation.Tests;

public class ConditionalTests
{
	[If(nameof(Toggle), true, 0)]
	public class SingleCondition
	{
		[Required]
		public bool Toggle { get; set; }

		[Required(ConditionGroup = 0)]
		public string Required { get; set; }
	}

	[Test]
	public void SingleConditionGeneration()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Toggle", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
				("Required", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Required("Toggle")
			.If(new JsonSchemaBuilder()
				.Properties(
					("Toggle", new JsonSchemaBuilder().Const(true))
				)
				.Required("Toggle")
			)
			.Then(new JsonSchemaBuilder().Required("Required"));

		VerifyGeneration<SingleCondition>(expected);
	}

	[Test]
	public void GenerationHonorsNamingMethod()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("toggle", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
				("required", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Required("toggle")
			.If(new JsonSchemaBuilder()
				.Properties(
					("toggle", new JsonSchemaBuilder().Const(true))
				)
				.Required("toggle")
			)
			.Then(new JsonSchemaBuilder().Required("required"));

		VerifyGeneration<SingleCondition>(expected, new SchemaGeneratorConfiguration
		{
			PropertyNameResolver = PropertyNameResolvers.CamelCase
		});
	}


	[Test]
	public void SingleConditionStrictGeneration()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Toggle", new JsonSchemaBuilder().Type(SchemaValueType.Boolean))
			)
			.Required("Toggle")
			.If(new JsonSchemaBuilder()
				.Properties(
					("Toggle", new JsonSchemaBuilder().Const(true))
				)
				.Required("Toggle")
			)
			.Then(new JsonSchemaBuilder()
				.Properties(
					("Required", new JsonSchemaBuilder().Type(SchemaValueType.String))
				)
				.Required("Required")
			)
			.UnevaluatedProperties(false);

		VerifyGeneration<SingleCondition>(expected, new SchemaGeneratorConfiguration { StrictConditionals = true });
	}


	[If(nameof(Toggle), true, "ifToggle")]
	[If(nameof(OtherToggle), 42, "ifOtherToggle")]
	public class MultipleConditionGroups
	{
		[Required]
		public bool Toggle { get; set; }
		[Required]
		public int OtherToggle { get; set; }

		// if (toggle == true || otherToggle == 42)
		[Required(ConditionGroup = "ifToggle")]
		[Required(ConditionGroup = "ifOtherToggle")]
		public string Required { get; set; }

		// if (otherToggle == 42)
		[Required(ConditionGroup = "ifOtherToggle")]
		public string OtherRequired { get; set; }
	}

	[Test]
	public void MultipleConditionGroupsGeneration()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Toggle", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
				("OtherToggle", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("Required", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("OtherRequired", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Required("Toggle", "OtherToggle")
			.AllOf(
				new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Properties(
							("Toggle", new JsonSchemaBuilder().Const(true))
						)
						.Required("Toggle")
					)
					.Then(new JsonSchemaBuilder().Required("Required")),
				new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Properties(
							("OtherToggle", new JsonSchemaBuilder().Const(42))
						)
						.Required("OtherToggle")
					)
					.Then(new JsonSchemaBuilder().Required("Required", "OtherRequired"))
			);

		VerifyGeneration<MultipleConditionGroups>(expected);
	}

	[If(nameof(Toggle), true, 0)]
	[If(nameof(OtherToggle), 42, 0)]
	public class MultipleConditionsInTheSameGroup
	{
		[Required]
		public bool Toggle { get; set; }
		[Required]
		public int OtherToggle { get; set; }

		// if (toggle == true && otherToggle == 42)
		[Required(ConditionGroup = 0)]
		public string Required { get; set; }
	}

	[Test]
	public void MultipleConditionsInTheSameGroupGeneration()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Toggle", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
				("OtherToggle", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("Required", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Required("Toggle", "OtherToggle")
			.If(new JsonSchemaBuilder()
				.Properties(
					("Toggle", new JsonSchemaBuilder().Const(true)),
					("OtherToggle", new JsonSchemaBuilder().Const(42))
				)
				.Required("Toggle", "OtherToggle")
			)
			.Then(new JsonSchemaBuilder().Required("Required"));

		VerifyGeneration<MultipleConditionsInTheSameGroup>(expected);
	}

	[If(nameof(Toggle), true, 0)]
	public class SingleConditionMultipleProperties
	{
		[Required]
		public bool Toggle { get; set; }

		// if (toggle == true)
		[Required(ConditionGroup = 0)]
		public string Required { get; set; }

		// if (toggle == true)
		[Required(ConditionGroup = 0)]
		public string OtherRequired { get; set; }
	}

	[Test]
	public void SingleConditionMultiplePropertiesGeneration()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Toggle", new JsonSchemaBuilder().Type(SchemaValueType.Boolean)),
				("Required", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("OtherRequired", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Required("Toggle")
			.If(new JsonSchemaBuilder()
				.Properties(
					("Toggle", new JsonSchemaBuilder().Const(true))
				)
				.Required("Toggle")
			)
			.Then(new JsonSchemaBuilder().Required("Required", "OtherRequired"));

		VerifyGeneration<SingleConditionMultipleProperties>(expected);
	}

	[IfEnum(nameof(Day))] // generates group for each value
	public class EnumSwitch
	{
		[Required]
		public DayOfWeek Day { get; set; }

		// if (day == Monday)
		[Required(ConditionGroup = DayOfWeek.Monday)]
		public string Required { get; set; }

		// if (toggle == Tuesday)
		[Required(ConditionGroup = DayOfWeek.Tuesday)]
		public string OtherRequired { get; set; }
	}

	[Test]
	public void EnumSwitchGeneration()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Day", new JsonSchemaBuilder()
					.Enum("Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday")
				),
				("Required", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("OtherRequired", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Required("Day")
			.AllOf(
				new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Properties(
							("Day", new JsonSchemaBuilder().Const("Monday"))
						)
						.Required("Day")
					)
					.Then(new JsonSchemaBuilder().Required("Required")),
				new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Properties(
							("Day", new JsonSchemaBuilder().Const("Tuesday"))
						)
						.Required("Day")
					)
					.Then(new JsonSchemaBuilder().Required("OtherRequired"))
			);

		VerifyGeneration<EnumSwitch>(expected);
	}

	[If(nameof(Value), "unused", 0)]
	public class UnusedConditionGroup
	{
		public string Value { get; set; }
	}

	[Test]
	public void UnusedGroupIsIgnored()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Value", new JsonSchemaBuilder().Type(SchemaValueType.String))
			);

		VerifyGeneration<UnusedConditionGroup>(expected);
	}

	public class UnknownConditionGroup
	{
		[Required(ConditionGroup = "unknown")]
		public string Value { get; set; }
	}

	[Test]
	public void UnknownGroupIsIgnored()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Value", new JsonSchemaBuilder().Type(SchemaValueType.String))
			);

		VerifyGeneration<UnknownConditionGroup>(expected);
	}

	[If(nameof(AgeCategory), "child", "isChild")]
	[If(nameof(AgeCategory), "adult", "isAdult")]
	[If(nameof(AgeCategory), "senior", "isSenior")]
	public class SplitAgeRanges
	{
		[Required]
		public string Name { get; set; }

		[Required]
		public string AgeCategory { get; set; }

		[Required]
		[Minimum(0, ConditionGroup = "isChild")]
		[Maximum(17, ConditionGroup = "isChild")]
		[Minimum(18, ConditionGroup = "isAdult")]
		[Maximum(64, ConditionGroup = "isAdult")]
		[Minimum(65, ConditionGroup = "isSenior")]
		public int Age { get; set; }

		[Required]
		[Const(false, ConditionGroup = "isChild")]
		[Const(true, ConditionGroup = "isAdult")]
		[Const(true, ConditionGroup = "isSenior")]
		public bool CanVote { get; set; }
	}

	[Test]
	public void SplitAgeRangesGeneration()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Name", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("AgeCategory", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("Age", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("CanVote", new JsonSchemaBuilder().Type(SchemaValueType.Boolean))
			)
			.Required("Name", "AgeCategory", "Age", "CanVote")
			.AllOf(
				new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Properties(
							("AgeCategory", new JsonSchemaBuilder().Const("child"))
						)
						.Required("AgeCategory")
					)
					.Then(new JsonSchemaBuilder()
						.Properties(
							("Age", new JsonSchemaBuilder()
								.Minimum(0)
								.Maximum(17)
							),
							("CanVote", new JsonSchemaBuilder()
								.Const(false)
							)
						)
					),
				new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Properties(
							("AgeCategory", new JsonSchemaBuilder().Const("adult"))
						)
						.Required("AgeCategory")
					)
					.Then(new JsonSchemaBuilder()
						.Properties(
							("Age", new JsonSchemaBuilder()
								.Minimum(18)
								.Maximum(64)
							),
							("CanVote", new JsonSchemaBuilder()
								.Const(true)
							)
						)
					),
				new JsonSchemaBuilder()
					.If(new JsonSchemaBuilder()
						.Properties(
							("AgeCategory", new JsonSchemaBuilder().Const("senior"))
						)
						.Required("AgeCategory")
					)
					.Then(new JsonSchemaBuilder()
						.Properties(
							("Age", new JsonSchemaBuilder()
								.Minimum(65)
							),
							("CanVote", new JsonSchemaBuilder()
								.Const(true)
							)
						)
					)
			);

		VerifyGeneration<SplitAgeRanges>(expected);
	}

	[IfMin(nameof(Value), 10, "group")]
	[IfMax(nameof(Value), 20, "group", IsExclusive = true)]
	public class NumberRangeConditions
	{
		[Required]
		public int Value { get; set; }

		[Required(ConditionGroup = "group")]
		public string Required { get; set; }
	}

	[Test]
	public void NumberRangeConditionsGeneration()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Value", new JsonSchemaBuilder().Type(SchemaValueType.Integer)),
				("Required", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Required("Value")
			.If(new JsonSchemaBuilder()
				.Properties(
					("Value", new JsonSchemaBuilder()
						.Minimum(10)
						.ExclusiveMaximum(20)
					)
				)
				.Required("Value")
			)
			.Then(new JsonSchemaBuilder()
				.Required("Required")
			);

		VerifyGeneration<NumberRangeConditions>(expected);
	}

	[IfMin(nameof(Value), 10, "group")]
	[IfMax(nameof(Value), 20, "group")]
	public class StringLengthRangeConditions
	{
		[Required]
		public string Value { get; set; }

		[Required(ConditionGroup = "group")]
		public string Required { get; set; }
	}

	[Test]
	public void StringLengthRangeConditionsGeneration()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Value", new JsonSchemaBuilder().Type(SchemaValueType.String)),
				("Required", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Required("Value")
			.If(new JsonSchemaBuilder()
				.Properties(
					("Value", new JsonSchemaBuilder()
						.MinLength(10)
						.MaxLength(20)
					)
				)
				.Required("Value")
			)
			.Then(new JsonSchemaBuilder()
				.Required("Required")
			);

		VerifyGeneration<StringLengthRangeConditions>(expected);
	}

	[IfMin(nameof(Value), 10, "group")]
	[IfMax(nameof(Value), 20, "group")]
	public class ArrayLengthRangeConditions
	{
		[Required]
		public int[] Value { get; set; }

		[Required(ConditionGroup = "group")]
		public string Required { get; set; }
	}

	[Test]
	public void ArrayLengthRangeConditionsGeneration()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Value", new JsonSchemaBuilder()
					.Type(SchemaValueType.Array)
					.Items(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
				),
				("Required", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Required("Value")
			.If(new JsonSchemaBuilder()
				.Properties(
					("Value", new JsonSchemaBuilder()
						.MinItems(10)
						.MaxItems(20)
					)
				)
				.Required("Value")
			)
			.Then(new JsonSchemaBuilder()
				.Required("Required")
			);

		VerifyGeneration<ArrayLengthRangeConditions>(expected);
	}

	[IfMin(nameof(Value), 10, "group")]
	[IfMax(nameof(Value), 20, "group")]
	public class DictionaryLengthRangeConditions
	{
		[Required]
		public Dictionary<string, int> Value { get; set; }

		[Required(ConditionGroup = "group")]
		public string Required { get; set; }
	}

	[Test]
	public void DictionaryLengthRangeConditionsGeneration()
	{
		JsonSchema expected = new JsonSchemaBuilder()
			.Type(SchemaValueType.Object)
			.Properties(
				("Value", new JsonSchemaBuilder()
					.Type(SchemaValueType.Object)
					.AdditionalProperties(new JsonSchemaBuilder().Type(SchemaValueType.Integer))
				),
				("Required", new JsonSchemaBuilder().Type(SchemaValueType.String))
			)
			.Required("Value")
			.If(new JsonSchemaBuilder()
				.Properties(
					("Value", new JsonSchemaBuilder()
						.MinProperties(10)
						.MaxProperties(20)
					)
				)
				.Required("Value")
			)
			.Then(new JsonSchemaBuilder()
				.Required("Required")
			);

		VerifyGeneration<DictionaryLengthRangeConditions>(expected);
	}
}