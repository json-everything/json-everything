using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Json.More.Tests
{
	public class EnumStringConverterTests
	{
		class ConversionTest
		{
			[JsonConverter(typeof(EnumStringConverter<DayOfWeek>))]
			[UsedImplicitly]
			public DayOfWeek Day { get; set; }
		}

		[Test]
		public void DayOfWeekAsPropertyIsConverted()
		{
			var expected = "{\"Day\":\"Wednesday\"}";
			var actual = JsonSerializer.Serialize(new ConversionTest {Day = DayOfWeek.Wednesday});

			Assert.AreEqual(expected, actual);
		}

		[JsonConverter(typeof(EnumStringConverter<CustomEnum>))]
		public enum CustomEnum
		{
			Zero,
			[System.ComponentModel.Description("one")]
			One,
			Two
		}

		[TestCase(CustomEnum.Zero, "Zero")]
		[TestCase(CustomEnum.One, "one")]
		[TestCase(CustomEnum.Two, "Two")]
		public void CustomEnumIsConverted(CustomEnum value, string serializedValue)
		{
			var expected = $"\"{serializedValue}\"";
			var actual = JsonSerializer.Serialize(value);

			Assert.AreEqual(expected, actual);
		}
	}
}