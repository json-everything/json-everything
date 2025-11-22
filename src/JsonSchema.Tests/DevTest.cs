using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class DevTest
{
	[Test]
	public void Test()
	{
		new Regex(@"\p{IsDigit}");
	}
}