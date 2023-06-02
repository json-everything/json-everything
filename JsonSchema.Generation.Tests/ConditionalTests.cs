using System;
using NUnit.Framework;

using static Json.Schema.Generation.Tests.AssertionExtensions;

namespace Json.Schema.Generation.Tests
{
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
				.AnyOf(
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
				.AnyOf(
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
	}
}
