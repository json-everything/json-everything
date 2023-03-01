using System.Text.Encodings.Web;
using System.Text.Json;
using NUnit.Framework;

namespace Json.More.Tests;

public class DevTest
{

	[Test]
	public void Test()
	{
		var options = new JsonSerializerOptions
		{
			Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
		};
	}
}