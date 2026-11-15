using System.Globalization;
using System.Text.Json;
using Json.More;
using NUnit.Framework;
using TestHelpers;

namespace Json.Schema.Generation.Tests;

public class Net10CompatibleRoundingTests
{
	// ReSharper disable once ClassNeverInstantiated.Local
	private class CompatibleRoundingSubject
	{
		[Minimum(0.1, true)]
		[Maximum(10.1, true)]
		[ExclusiveMinimum(1.1, true)]
		[ExclusiveMaximum(4.789, true)]
		[MultipleOf(3.14, true)]
		// ReSharper disable once UnusedMember.Local
		public decimal Value { get; set; }
	}

	// ReSharper disable once ClassNeverInstantiated.Local
	private class DefaultRoundingSubject
	{
		[Minimum(0.1)]
		[Maximum(10.1)]
		[ExclusiveMinimum(1.1)]
		[ExclusiveMaximum(4.789)]
		[MultipleOf(3.14)]
		// ReSharper disable once UnusedMember.Local
		public decimal Value { get; set; }
	}

	// ReSharper disable once ClassNeverInstantiated.Local
	private class ExactlyRepresentableSubject
	{
		[Minimum(0.5)]
		[Maximum(10.5)]
		[ExclusiveMinimum(0.25)]
		[ExclusiveMaximum(12)]
		[MultipleOf(0.125)]
		// ReSharper disable once UnusedMember.Local
		public decimal Value { get; set; }
	}

	[IfMin(nameof(Value), 0.1, "group", UseNet10CompatibleRounding = true)]
	[IfMax(nameof(Value), 4.789, "group", IsExclusive = true, UseNet10CompatibleRounding = true)]
	// ReSharper disable once ClassNeverInstantiated.Local
	private class CompatibleRoundingConditionSubject
	{
		// ReSharper disable once UnusedMember.Local
		public decimal Value { get; set; }

		[Required(ConditionGroup = "group")]
		// ReSharper disable once UnusedMember.Local
		public string? Other { get; set; }
	}

	[IfMin(nameof(Value), 0.1, "group")]
	[IfMax(nameof(Value), 4.789, "group", IsExclusive = true)]
	// ReSharper disable once ClassNeverInstantiated.Local
	private class DefaultRoundingConditionSubject
	{
		// ReSharper disable once UnusedMember.Local
		public decimal Value { get; set; }

		[Required(ConditionGroup = "group")]
		// ReSharper disable once UnusedMember.Local
		public string? Other { get; set; }
	}

	[Test]
	public void ConditionCompatibleRoundingProducesShortValues()
	{
		var builder = new JsonSchemaBuilder();
		builder.FromType<CompatibleRoundingConditionSubject>();

		var schema = builder.Build();
		TestConsole.WriteLine(schema.Root.Source);

		var condition = schema.Root.Source.GetProperty("if").GetProperty("properties").GetProperty("Value");

		Assert.Multiple(() =>
		{
			Assert.That(condition.GetProperty("minimum").GetDecimal(), Is.EqualTo(0.1m));
			Assert.That(condition.GetProperty("exclusiveMaximum").GetDecimal(), Is.EqualTo(4.789m));
		});
	}

	[Test]
	public void ConditionDefaultRoundingFollowsRuntime()
	{
		var builder = new JsonSchemaBuilder();
		builder.FromType<DefaultRoundingConditionSubject>();

		var schema = builder.Build();
		TestConsole.WriteLine(schema.Root.Source);

		var condition = schema.Root.Source.GetProperty("if").GetProperty("properties").GetProperty("Value");

#if NET11_0_OR_GREATER
		var expectedMinimum = 0.1000000000000000055511151231m;
		var expectedExclusiveMaximum = 4.7889999999999997015720509808m;
#else
		var expectedMinimum = 0.1m;
		var expectedExclusiveMaximum = 4.789m;
#endif

		Assert.Multiple(() =>
		{
			Assert.That(condition.GetProperty("minimum").GetDecimal(), Is.EqualTo(expectedMinimum));
			Assert.That(condition.GetProperty("exclusiveMaximum").GetDecimal(), Is.EqualTo(expectedExclusiveMaximum));
		});
	}

	[Test]
	public void CompatibleRoundingProducesShortValues()
	{
		var builder = new JsonSchemaBuilder();
		builder.FromType<CompatibleRoundingSubject>();

		var schema = builder.Build();
		TestConsole.WriteLine(schema.Root.Source);

		var expected = JsonDocument.Parse(
			"""
			{
			  "$schema": "https://json-schema.org/draft/2020-12/schema",
			  "type": "object",
			  "properties": {
			    "Value": {
			      "type": "number",
			      "minimum": 0.1,
			      "maximum": 10.1,
			      "exclusiveMinimum": 1.1,
			      "exclusiveMaximum": 4.789,
			      "multipleOf": 3.14
			    }
			  }
			}
			""").RootElement;

		Assert.That(expected.IsEquivalentTo(schema.Root.Source));
	}

	[Test]
	public void DefaultRoundingFollowsRuntime()
	{
		var builder = new JsonSchemaBuilder();
		builder.FromType<DefaultRoundingSubject>();

		var schema = builder.Build();
		TestConsole.WriteLine(schema.Root.Source);

#if NET11_0_OR_GREATER
		var expected = JsonDocument.Parse(
			"""
			{
			  "$schema": "https://json-schema.org/draft/2020-12/schema",
			  "type": "object",
			  "properties": {
			    "Value": {
			      "type": "number",
			      "minimum": 0.1000000000000000055511151231,
			      "maximum": 10.099999999999999644728632120,
			      "exclusiveMinimum": 1.1000000000000000888178419700,
			      "exclusiveMaximum": 4.7889999999999997015720509808,
			      "multipleOf": 3.1400000000000001243449787580
			    }
			  }
			}
			""").RootElement;
#else
		var expected = JsonDocument.Parse(
			"""
			{
			  "$schema": "https://json-schema.org/draft/2020-12/schema",
			  "type": "object",
			  "properties": {
			    "Value": {
			      "type": "number",
			      "minimum": 0.1,
			      "maximum": 10.1,
			      "exclusiveMinimum": 1.1,
			      "exclusiveMaximum": 4.789,
			      "multipleOf": 3.14
			    }
			  }
			}
			""").RootElement;
#endif

		Assert.That(expected.IsEquivalentTo(schema.Root.Source));
	}

	[Test]
	public void ExactlyRepresentableValuesAreUnaffected()
	{
		var builder = new JsonSchemaBuilder();
		builder.FromType<ExactlyRepresentableSubject>();

		var schema = builder.Build();
		TestConsole.WriteLine(schema.Root.Source);

		var expected = JsonDocument.Parse(
			"""
			{
			  "$schema": "https://json-schema.org/draft/2020-12/schema",
			  "type": "object",
			  "properties": {
			    "Value": {
			      "type": "number",
			      "minimum": 0.5,
			      "maximum": 10.5,
			      "exclusiveMinimum": 0.25,
			      "exclusiveMaximum": 12,
			      "multipleOf": 0.125
			    }
			  }
			}
			""").RootElement;

		Assert.That(expected.IsEquivalentTo(schema.Root.Source));
	}

	[TestCase(0, "0")]
	[TestCase(5, "5")]
	[TestCase(10.5, "10.5")]
	[TestCase(0.1, "0.1")]
	[TestCase(-0.1, "-0.1")]
	[TestCase(10.1, "10.1")]
	[TestCase(4.789, "4.789")]
	[TestCase(0.00001, "0.00001")]
	[TestCase(123456789.123456789, "123456789.123457")]
	[TestCase(1234567890123456789, "1234567890123460000")]
	public void CompatibleRoundingKeepsFifteenSignificantDigits(double value, string expectedText)
	{
		var expected = decimal.Parse(expectedText, CultureInfo.InvariantCulture);

		Assert.Multiple(() =>
		{
			Assert.That(new MinimumAttribute(value, true).Value, Is.EqualTo(expected));
			Assert.That(new MaximumAttribute(value, true).Value, Is.EqualTo(expected));
			Assert.That(new ExclusiveMinimumAttribute(value, true).Value, Is.EqualTo(expected));
			Assert.That(new ExclusiveMaximumAttribute(value, true).Value, Is.EqualTo(expected));
			Assert.That(new MultipleOfAttribute(value, true).Value, Is.EqualTo(expected));
		});
	}
}
