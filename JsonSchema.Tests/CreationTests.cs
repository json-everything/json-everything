using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Json.Schema.Tests;

public class CreationTests
{
	[Test]
	public async Task FromText()
	{
		var schema = JsonSchema.FromText("{\"$id\":\"http://my.schema/test1\",\"minimum\":5}");

		var results = await schema.Evaluate(10);

		Assert.True(results.IsValid);
	}
	[Test]
	public async Task FromTextIgnoringComments()
	{
		var options = new JsonSerializerOptions { ReadCommentHandling = JsonCommentHandling.Skip };
		var schema = JsonSchema.FromText(@"{
  ""$id"":""http://my.schema/test1"",
  // comment here, just passing through
  ""minimum"":5
}", options);

		using var json = JsonDocument.Parse("10");

		var results = await schema.Evaluate(json.RootElement);

		Assert.True(results.IsValid);
	}
	[Test]
	public async Task FromStream()
	{
		var text = "{\"$id\":\"http://my.schema/test1\",\"minimum\":5}";
		using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));

		var schema = await JsonSchema.FromStream(stream);

		using var json = JsonDocument.Parse("10");

		var results = await schema.Evaluate(json.RootElement);

		Assert.True(results.IsValid);
	}
}