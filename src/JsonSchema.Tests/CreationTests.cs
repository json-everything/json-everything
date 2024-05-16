using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class CreationTests
{
	[Test]
	public void FromText()
	{
		var schema = JsonSchema.FromText("{\"$id\":\"http://my.schema/test1\",\"minimum\":5}");

		var results = schema.Evaluate(10);

		results.AssertValid();
	}
	[Test]
	public void FromTextIgnoringComments()
	{
		var options = new JsonSerializerOptions
		{
			TypeInfoResolverChain = { TestSerializerContext.Default },
			ReadCommentHandling = JsonCommentHandling.Skip
		};
		var schema = JsonSchema.FromText(@"{
  ""$id"":""http://my.schema/test1"",
  // comment here, just passing through
  ""minimum"":5
}", options);

		using var json = JsonDocument.Parse("10");

		var results = schema.Evaluate(json.RootElement);

		results.AssertValid();
	}
	[Test]
	public async Task FromStream()
	{
		var text = "{\"$id\":\"http://my.schema/test1\",\"minimum\":5}";
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));

		var schema = await JsonSchema.FromStream(stream);

		using var json = JsonDocument.Parse("10");

		var results = schema.Evaluate(json.RootElement);

		results.AssertValid();
	}
}