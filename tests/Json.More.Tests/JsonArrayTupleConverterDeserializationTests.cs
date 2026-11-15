using System;
using System.Text.Json;
using NUnit.Framework;

namespace Json.More.Tests;

public class JsonArrayTupleConverterDeserializationTests
{
	private static readonly JsonSerializerOptions _options = new()
	{
		Converters = { new JsonArrayTupleConverter() }
	};

	[Test]
	public void OneValue()
	{
		var expected = ValueTuple.Create(1);
		var json = "[1]";

		var actual = JsonSerializer.Deserialize<ValueTuple<int>>(json, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TwoValues()
	{
		var expected = (1, "string");
		var json = "[1,\"string\"]";

		var actual = JsonSerializer.Deserialize<(int, string)>(json, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void ThreeValues()
	{
		var expected = (1, "string", false);
		var json = "[1,\"string\",false]";

		var actual = JsonSerializer.Deserialize<(int, string, bool)>(json, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void FourValues()
	{
		var expected = (1, "string", false, -4.2);
		var json = "[1,\"string\",false,-4.2]";

		var actual = JsonSerializer.Deserialize<(int, string, bool, double)>(json, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void FiveValues()
	{
		var expected = (1, "string", false, -4.2, "foo");
		var json = "[1,\"string\",false,-4.2,\"foo\"]";

		var actual = JsonSerializer.Deserialize<(int, string, bool, double, string)>(json, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void SixValues()
	{
		var expected = (1, "string", false, -4.2, "foo", 6);
		var json = "[1,\"string\",false,-4.2,\"foo\",6]";

		var actual = JsonSerializer.Deserialize<(int, string, bool, double, string, int)>(json, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void SevenValues()
	{
		var expected = (1, "string", false, -4.2, "foo", 6, 7);
		var json = "[1,\"string\",false,-4.2,\"foo\",6,7]";

		var actual = JsonSerializer.Deserialize<(int, string, bool, double, string, int, int)>(json, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void EightValues()
	{
		var expected = (1, "string", false, -4.2, "foo", 6, 7, 8);
		var json = "[1,\"string\",false,-4.2,\"foo\",6,7,8]";

		var actual = JsonSerializer.Deserialize<(int, string, bool, double, string, int, int, int)>(json, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void MoreValues()
	{
		var expected = (1, "string", false, -4.2, "foo", 6, 7, 8, 9, 10, 11, 12);
		var json = "[1,\"string\",false,-4.2,\"foo\",6,7,8,9,10,11,12]";

		var actual = JsonSerializer.Deserialize<(int, string, bool, double, string, int, int, int, int, int, int, int)>(json, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TupleInObject()
	{
		var expected = (false, new ObjectWithTuple { Tuple = (42, "foo") });
		var json = "[false,{\"Tuple\":[42,\"foo\"]}]";

		var actual = JsonSerializer.Deserialize<(bool, ObjectWithTuple)>(json, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TupleInArray()
	{
		(int, string, bool, double, string, int, int, int)[] expected = [(1, "string", false, -4.2, "foo", 6, 7, 8), (10, "bool", true, 4.2, "bar", 6, 7, 8)];
		var json = "[[1,\"string\",false,-4.2,\"foo\",6,7,8],[10,\"bool\",true,4.2,\"bar\",6,7,8]]";

		var actual = JsonSerializer.Deserialize<(int, string, bool, double, string, int, int, int)[]>(json, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void NullInTuple()
	{
		var expected = new ValueTuple<string>(null!);
		var actual = JsonSerializer.Deserialize<ValueTuple<string>>("[null]", new JsonSerializerOptions
		{
			Converters = { new JsonArrayTupleConverter() }
		});

		Assert.That(actual, Is.EqualTo(expected));
	}
}