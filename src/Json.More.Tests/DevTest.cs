using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;

namespace Json.More.Tests;

public class DevTest
{
#if NET9_0_OR_GREATER
	public enum Issue890_Status
	{
		[JsonStringEnumMemberName("active")]
		Active,
		[JsonStringEnumMemberName("inactive")]
		Inactive
	}

	public class Container
	{
		public Issue890_Status Enum { get; set; }
	}

	[Test]
	public void Test()
	{
		var options = new JsonSerializerOptions
		{
			WriteIndented = true,
			Converters = { new JsonStringEnumConverter() }
		};

		var value = Issue890_Status.Inactive;
		var objValue = (object)value;

		var contained = new Container { Enum = Issue890_Status.Inactive };

		Console.WriteLine(JsonSerializer.Serialize(value, options));
		Console.WriteLine(JsonSerializer.Serialize(objValue, options));
		Console.WriteLine(JsonSerializer.Serialize(contained, options));
	}
#endif
}