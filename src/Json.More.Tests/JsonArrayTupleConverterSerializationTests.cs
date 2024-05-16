using System;
using System.Text.Json;
using NUnit.Framework;

namespace Json.More.Tests;

public class JsonArrayTupleConverterSerializationTests
{
	private static readonly JsonSerializerOptions _options = new()
	{
		Converters = { new JsonArrayTupleConverter() }
	};

	[Test]
	public void OneValue()
	{
		var tuple = ValueTuple.Create(1);
		var expected = "[1]";

		var actual = JsonSerializer.Serialize(tuple, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TwoValues()
	{
		var tuple = (1, "string");
		var expected = "[1,\"string\"]";

		var actual = JsonSerializer.Serialize(tuple, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void ThreeValues()
	{
		var tuple = (1, "string", false);
		var expected = "[1,\"string\",false]";

		var actual = JsonSerializer.Serialize(tuple, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void FourValues()
	{
		var tuple = (1, "string", false, -4.2);
		var expected = "[1,\"string\",false,-4.2]";

		var actual = JsonSerializer.Serialize(tuple, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void FiveValues()
	{
		var tuple = (1, "string", false, -4.2, "foo");
		var expected = "[1,\"string\",false,-4.2,\"foo\"]";

		var actual = JsonSerializer.Serialize(tuple, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void SixValues()
	{
		var tuple = (1, "string", false, -4.2, "foo", 6);
		var expected = "[1,\"string\",false,-4.2,\"foo\",6]";

		var actual = JsonSerializer.Serialize(tuple, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void SevenValues()
	{
		var tuple = (1, "string", false, -4.2, "foo", 6, 7);
		var expected = "[1,\"string\",false,-4.2,\"foo\",6,7]";

		var actual = JsonSerializer.Serialize(tuple, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void EightValues()
	{
		var tuple = (1, "string", false, -4.2, "foo", 6, 7, 8);
		var expected = "[1,\"string\",false,-4.2,\"foo\",6,7,8]";

		var actual = JsonSerializer.Serialize(tuple, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void MoreValues()
	{
		var tuple = (1, "string", false, -4.2, "foo", 6, 7, 8, 9, 10, 11, 12);
		var expected = "[1,\"string\",false,-4.2,\"foo\",6,7,8,9,10,11,12]";

		var actual = JsonSerializer.Serialize(tuple, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}

	[Test]
	public void TupleInObject()
	{
		var tuple = (false, new ObjectWithTuple { Tuple = (42, "foo") });
		var expected = "[false,{\"Tuple\":[42,\"foo\"]}]";

		var actual = JsonSerializer.Serialize(tuple, _options);

		Assert.That(actual, Is.EqualTo(expected));
	}
}