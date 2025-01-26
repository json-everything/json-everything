using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;

namespace Json.More.Tests;

public class EnumStringConverterTests
{
	private class ConversionTest
	{
		[JsonConverter(typeof(EnumStringConverter<DayOfWeek>))]
		public DayOfWeek Day { get; set; }
	}

	[Test]
	public void DayOfWeekAsPropertyIsConverted()
	{
		var expected = "{\"Day\":\"Wednesday\"}";
		var actual = JsonSerializer.Serialize(new ConversionTest { Day = DayOfWeek.Wednesday });

		Assert.That(actual, Is.EqualTo(expected));

		var deserialized = JsonSerializer.Deserialize<ConversionTest>(actual)!;

		Assert.That(deserialized.Day, Is.EqualTo(DayOfWeek.Wednesday));
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

		Assert.That(actual, Is.EqualTo(expected));

		var deserialized = JsonSerializer.Deserialize<CustomEnum>(actual);

		Assert.That(deserialized, Is.EqualTo(value));
	}

	[JsonConverter(typeof(EnumStringConverter<CustomFlagsEnum>))]
	[Flags]
	private enum CustomFlagsEnum
	{
		Zero,
		One = 1,
		Two = 2
	}

	private class FlagsEnumContainer
	{
		public CustomFlagsEnum Value { get; set; }
	}

	[Test]
	public void CustomFlagsEnumIsConverted()
	{
		var value = new FlagsEnumContainer { Value = CustomFlagsEnum.One | CustomFlagsEnum.Two };

		var expected = "{\"Value\":[\"One\",\"Two\"]}";
		var actual = JsonSerializer.Serialize(value);

		Assert.That(actual, Is.EqualTo(expected));

		var deserialized = JsonSerializer.Deserialize<FlagsEnumContainer>(actual)!;

		Assert.That(deserialized.Value, Is.EqualTo(CustomFlagsEnum.One | CustomFlagsEnum.Two));
	}

	private class EnumWithDuplicatedMemberValuesContainer
	{
		[JsonConverter(typeof(EnumStringConverter<HttpStatusCode>))]
		public HttpStatusCode Value { get; set; }
	}

	[Test]
	public void EnumWithDuplicatedMemberValuesIsConverted()
	{
		var value = new EnumWithDuplicatedMemberValuesContainer { Value = HttpStatusCode.MultipleChoices };

		var expected = "{\"Value\":\"MultipleChoices\"}";
		var actual = JsonSerializer.Serialize(value);

		Assert.That(actual, Is.EqualTo(expected));

		var deserialized = JsonSerializer.Deserialize<EnumWithDuplicatedMemberValuesContainer>(actual)!;

		Assert.That(deserialized.Value, Is.EqualTo(HttpStatusCode.MultipleChoices));
	}
}