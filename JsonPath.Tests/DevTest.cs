using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Path.Tests;

internal class DevTest
{
	[Test]
	public void Test()
	{
		var date = "2023-08-07";
		var otherDate = "2023-05-01";

		Assert.AreEqual(1, string.Compare(date, otherDate, StringComparison.Ordinal));
	}
}