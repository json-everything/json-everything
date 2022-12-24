using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using NUnit.Framework;

namespace Json.Path.Tests;

public class ParsedGoessnerTests
{
	private readonly JsonNode _instance;

	public ParsedGoessnerTests()
	{
		var model = new Repository
		{
			Store = new Store
			{
				Book = new[]
				{
					new Book
					{
						Category = "reference",
						Author = "Nigel Rees",
						Title = "Sayings of the Century",
						Price = 8.95m
					},
					new Book
					{
						Category = "fiction",
						Author = "Evelyn Waugh",
						Title = "Sword of Honour",
						Price = 12.99m
					},
					new Book
					{
						Category = "fiction",
						Author = "Herman Melville",
						Title = "Moby Dick",
						Isbn = "0-553-21311-3",
						Price = 8.99m
					},
					new Book
					{
						Category = "fiction",
						Author = "J. R. R. Tolkien",
						Title = "The Lord of the Rings",
						Isbn = "0-395-19395-8",
						Price = 22.99m
					}
				},
				Bicycle = new Bicycle
				{
					Color = "red",
					Price = 19.95m
				}
			}
		};

		_instance = JsonSerializer.SerializeToNode(model, new JsonSerializerOptions
		{
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		})!;
	}

	[Test]
	public void GrammarExample()
	{
		var input = "$.store.book[0].title";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.IsNull(result.Error);
		Assert.AreEqual(1, result.Matches!.Count);
		Assert.AreEqual("Sayings of the Century", result.Matches[0].Value!.GetValue<string>());
		Assert.AreEqual(input, path.ToString());
	}

	[Test]
	public void Example1()
	{
		var input = "$.store.book[*].author";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.IsNull(result.Error);
		Assert.AreEqual(4, result.Matches!.Count);
		Assert.AreEqual("Nigel Rees", result.Matches[0].Value!.GetValue<string>());
		Assert.AreEqual("Evelyn Waugh", result.Matches[1].Value!.GetValue<string>());
		Assert.AreEqual("Herman Melville", result.Matches[2].Value!.GetValue<string>());
		Assert.AreEqual("J. R. R. Tolkien", result.Matches[3].Value!.GetValue<string>());
		Assert.AreEqual(input, path.ToString());
	}

	[Test]
	public void Example2()
	{
		var input = "$..author";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.IsNull(result.Error);
		Assert.AreEqual(4, result.Matches!.Count);
		Assert.AreEqual("Nigel Rees", result.Matches[0].Value!.GetValue<string>());
		Assert.AreEqual("Evelyn Waugh", result.Matches[1].Value!.GetValue<string>());
		Assert.AreEqual("Herman Melville", result.Matches[2].Value!.GetValue<string>());
		Assert.AreEqual("J. R. R. Tolkien", result.Matches[3].Value!.GetValue<string>());
		Assert.AreEqual(input, path.ToString());
	}

	[Test]
	public void Example3()
	{
		var input = "$.store.*";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.IsNull(result.Error);
		Assert.AreEqual(2, result.Matches!.Count);
		Assert.AreEqual(4, result.Matches[0].Value!.AsArray().Count);
		Assert.AreEqual(2, result.Matches[1].Value!.AsObject().Count);
		Assert.AreEqual(input, path.ToString());
	}

	[Test]
	public void Example4()
	{
		var input = "$.store..price";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.IsNull(result.Error);
		Assert.AreEqual(5, result.Matches!.Count);
		Assert.AreEqual(8.95m, result.Matches[0].Value!.AsValue().GetNumber());
		Assert.AreEqual(12.99m, result.Matches[1].Value!.AsValue().GetNumber());
		Assert.AreEqual(8.99m, result.Matches[2].Value!.AsValue().GetNumber());
		Assert.AreEqual(22.99m, result.Matches[3].Value!.AsValue().GetNumber());
		Assert.AreEqual(19.95m, result.Matches[4].Value!.AsValue().GetNumber());
		Assert.AreEqual(input, path.ToString());
	}

	[Test]
	public void Example5()
	{
		var input = "$..book[2]";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.IsNull(result.Error);
		Assert.AreEqual(1, result.Matches!.Count);
		Assert.AreEqual("Moby Dick", result.Matches[0].Value!["title"]!.GetValue<string>());
		Assert.AreEqual(input, path.ToString());
	}

	[Test]
	[Ignore("This syntax is no longer valid")]
	public void Example6A()
	{
		var input = "$..book[(@.length-1)]";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.IsNull(result.Error);
		Assert.AreEqual(1, result.Matches!.Count);
		Assert.AreEqual("The Lord of the Rings", result.Matches[0].Value!["title"]!.GetValue<string>());
		Assert.AreEqual(input, path.ToString());
	}

	[Test]
	public void Example6B()
	{
		var input = "$..book[-1:]";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.IsNull(result.Error);
		Assert.AreEqual(1, result.Matches!.Count);
		Assert.AreEqual("The Lord of the Rings", result.Matches[0].Value!["title"]!.GetValue<string>());
		Assert.AreEqual(input, path.ToString());
	}

	[Test]
	public void Example7A()
	{
		var input = "$..book[0,1]";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.IsNull(result.Error);
		Assert.AreEqual(2, result.Matches!.Count);
		Assert.AreEqual("Sayings of the Century", result.Matches[0].Value!["title"]!.GetValue<string>());
		Assert.AreEqual("Sword of Honour", result.Matches[1].Value!["title"]!.GetValue<string>());
		Assert.AreEqual(input, path.ToString());
	}

	[Test]
	public void Example7B()
	{
		var input = "$..book[:2]";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.IsNull(result.Error);
		Assert.AreEqual(2, result.Matches!.Count);
		Assert.AreEqual("Sayings of the Century", result.Matches[0].Value!["title"]!.GetValue<string>());
		Assert.AreEqual("Sword of Honour", result.Matches[1].Value!["title"]!.GetValue<string>());
		Assert.AreEqual(input, path.ToString());
	}

	[Test]
	public void Example8()
	{
		var input = "$..book[?(@.isbn)]";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.IsNull(result.Error);
		Assert.AreEqual(2, result.Matches!.Count);
		Assert.AreEqual("Moby Dick", result.Matches[0].Value!["title"]!.GetValue<string>());
		Assert.AreEqual("The Lord of the Rings", result.Matches[1].Value!["title"]!.GetValue<string>());
		Assert.AreEqual(input, path.ToString());
	}

	[Test]
	public void Example9()
	{
		var input = "$..book[?(@.price<10)]";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.IsNull(result.Error);
		Assert.AreEqual(2, result.Matches!.Count);
		Assert.AreEqual("Sayings of the Century", result.Matches[0].Value!["title"]!.GetValue<string>());
		Assert.AreEqual("Moby Dick", result.Matches[1].Value!["title"]!.GetValue<string>());
		Assert.AreEqual(input, path.ToString());
	}

	[Test]
	public void Example10()
	{
		var input = "$..*";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.IsNull(result.Error);
		Assert.AreEqual(27, result.Matches!.Count);
		Assert.AreEqual(input, path.ToString());
	}
}