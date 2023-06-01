using System;
using NUnit.Framework;

namespace Json.Schema.Generation.Tests
{
	public class ConditionalTests
	{
		[If(nameof(Toggle), true, 0)]
		public class SingleCondition
		{
			public bool Toggle { get; set; }

			[Required(ConditionGroup = 0)]
			public string Required { get; set; }
		}

		[If(nameof(Toggle), true, "ifToggle")]
		[If(nameof(OtherToggle), 42, "ifOtherToggle")]
		public class MultipleConditionGroups
		{
			public bool Toggle { get; set; }
			public int OtherToggle { get; set; }

			// if (toggle == true || otherToggle == 42)
			[Required(ConditionGroup = "ifToggle")]
			[Required(ConditionGroup = "ifOtherToggle")]
			public string Required { get; set; }

			// if (otherToggle == 42)
			[Required(ConditionGroup = "ifOtherToggle")]
			public string OtherRequired { get; set; }
		}

		[If(nameof(Toggle), true, 0)]
		[If(nameof(OtherToggle), 42, 0)]
		public class MultipleConditionsInTheSameGroup
		{
			public bool Toggle { get; set; }
			public int OtherToggle { get; set; }

			// if (toggle == true && otherToggle == 42)
			[Required(ConditionGroup = 0)]
			public string Required { get; set; }
		}

		[If(nameof(Toggle), true, 0)]
		public class SingleConditionMultipleProperties
		{
			public bool Toggle { get; set; }

			// if (toggle == true)
			[Required(ConditionGroup = 0)]
			public string Required { get; set; }

			// if (toggle == true)
			[Required(ConditionGroup = 0)]
			public string OtherRequired { get; set; }
		}

		[IfEnum(nameof(Day))] // generates group for each value
		public class EnumSwitch
		{
			public DayOfWeek Day { get; set; }

			// if (day == Monday)
			[Required(ConditionGroup = DayOfWeek.Monday)]
			public string Required { get; set; }

			// if (toggle == Tuesday)
			[Required(ConditionGroup = DayOfWeek.Tuesday)]
			public string OtherRequired { get; set; }
		}

		[Test]
		public void GenerateConditionalBranches()
		{

		}
	}
}
