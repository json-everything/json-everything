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
				Book =
				[
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
				],
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

		Assert.Multiple(() =>
		{
			//Assert.That(result.Error, Is.Null);
			Assert.That(result.Matches!, Has.Count.EqualTo(1));
			Assert.That(result.Matches![0].Value!.GetValue<string>(), Is.EqualTo("Sayings of the Century"));
			Assert.That(path.ToString(), Is.EqualTo(input));
		});
	}

	[Test]
	public void Example1()
	{
		var input = "$.store.book[*].author";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.Multiple(() =>
		{
			//Assert.That(result.Error, Is.Null);
			Assert.That(result.Matches!, Has.Count.EqualTo(4));
			Assert.That(result.Matches![0].Value!.GetValue<string>(), Is.EqualTo("Nigel Rees"));
			Assert.That(result.Matches[1].Value!.GetValue<string>(), Is.EqualTo("Evelyn Waugh"));
			Assert.That(result.Matches[2].Value!.GetValue<string>(), Is.EqualTo("Herman Melville"));
			Assert.That(result.Matches[3].Value!.GetValue<string>(), Is.EqualTo("J. R. R. Tolkien"));
			Assert.That(path.ToString(), Is.EqualTo(input));
		});
	}

	[Test]
	public void Example2()
	{
		var input = "$..author";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.Multiple(() =>
		{
			//Assert.That(result.Error, Is.Null);
			Assert.That(result.Matches!, Has.Count.EqualTo(4));
			Assert.That(result.Matches![0].Value!.GetValue<string>(), Is.EqualTo("Nigel Rees"));
			Assert.That(result.Matches[1].Value!.GetValue<string>(), Is.EqualTo("Evelyn Waugh"));
			Assert.That(result.Matches[2].Value!.GetValue<string>(), Is.EqualTo("Herman Melville"));
			Assert.That(result.Matches[3].Value!.GetValue<string>(), Is.EqualTo("J. R. R. Tolkien"));
			Assert.That(path.ToString(), Is.EqualTo(input));
		});
	}

	[Test]
	public void Example3()
	{
		var input = "$.store.*";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.Multiple(() =>
		{
			//Assert.That(result.Error, Is.Null);
			Assert.That(result.Matches!, Has.Count.EqualTo(2));
			Assert.That(result.Matches![0].Value!.AsArray(), Has.Count.EqualTo(4));
			Assert.That(result.Matches[1].Value!.AsObject(), Has.Count.EqualTo(2));
			Assert.That(path.ToString(), Is.EqualTo(input));
		});
	}

	[Test]
	public void Example4()
	{
		var input = "$.store..price";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.Multiple(() =>
		{
			//Assert.That(result.Error, Is.Null);
			Assert.That(result.Matches!, Has.Count.EqualTo(5));
			Assert.That(result.Matches![0].Value!.AsValue().GetNumber(), Is.EqualTo(8.95m));
			Assert.That(result.Matches[1].Value!.AsValue().GetNumber(), Is.EqualTo(12.99m));
			Assert.That(result.Matches[2].Value!.AsValue().GetNumber(), Is.EqualTo(8.99m));
			Assert.That(result.Matches[3].Value!.AsValue().GetNumber(), Is.EqualTo(22.99m));
			Assert.That(result.Matches[4].Value!.AsValue().GetNumber(), Is.EqualTo(19.95m));
			Assert.That(path.ToString(), Is.EqualTo(input));
		});
	}

	[Test]
	public void Example5()
	{
		var input = "$..book[2]";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.Multiple(() =>
		{
			//Assert.That(result.Error, Is.Null);
			Assert.That(result.Matches!, Has.Count.EqualTo(1));
			Assert.That(result.Matches![0].Value!["title"]!.GetValue<string>(), Is.EqualTo("Moby Dick"));
			Assert.That(path.ToString(), Is.EqualTo(input));
		});
	}

	[Test]
	[Ignore("This syntax is no longer valid")]
	public void Example6A()
	{
		var input = "$..book[(@.length-1)]";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.Multiple(() =>
		{
			//Assert.That(result.Error, Is.Null);
			Assert.That(result.Matches!, Has.Count.EqualTo(1));
			Assert.That(result.Matches![0].Value!["title"]!.GetValue<string>(), Is.EqualTo("The Lord of the Rings"));
			Assert.That(path.ToString(), Is.EqualTo(input));
		});
	}

	[Test]
	public void Example6B()
	{
		var input = "$..book[-1:]";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.Multiple(() =>
		{
			//Assert.That(result.Error, Is.Null);
			Assert.That(result.Matches!, Has.Count.EqualTo(1));
			Assert.That(result.Matches![0].Value!["title"]!.GetValue<string>(), Is.EqualTo("The Lord of the Rings"));
			Assert.That(path.ToString(), Is.EqualTo(input));
		});
	}

	[Test]
	public void Example7A()
	{
		var input = "$..book[0,1]";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.Multiple(() =>
		{
			//Assert.That(result.Error, Is.Null);
			Assert.That(result.Matches!, Has.Count.EqualTo(2));
			Assert.That(result.Matches![0].Value!["title"]!.GetValue<string>(), Is.EqualTo("Sayings of the Century"));
			Assert.That(result.Matches[1].Value!["title"]!.GetValue<string>(), Is.EqualTo("Sword of Honour"));
			Assert.That(path.ToString(), Is.EqualTo(input));
		});
	}

	[Test]
	public void Example7B()
	{
		var input = "$..book[:2]";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.Multiple(() =>
		{
			//Assert.That(result.Error, Is.Null);
			Assert.That(result.Matches!, Has.Count.EqualTo(2));
			Assert.That(result.Matches![0].Value!["title"]!.GetValue<string>(), Is.EqualTo("Sayings of the Century"));
			Assert.That(result.Matches[1].Value!["title"]!.GetValue<string>(), Is.EqualTo("Sword of Honour"));
			Assert.That(path.ToString(), Is.EqualTo(input));
		});
	}

	[Test]
	public void Example8()
	{
		var input = "$..book[?(@.isbn)]";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.Multiple(() =>
		{
			//Assert.That(result.Error, Is.Null);
			Assert.That(result.Matches!, Has.Count.EqualTo(2));
			Assert.That(result.Matches![0].Value!["title"]!.GetValue<string>(), Is.EqualTo("Moby Dick"));
			Assert.That(result.Matches[1].Value!["title"]!.GetValue<string>(), Is.EqualTo("The Lord of the Rings"));
			// the parens have been deemed unnecessary now.
			//Assert.AreEqual(input, path.ToString());
		});
	}

	[Test]
	public void Example9()
	{
		var input = "$..book[?(@.price<10)]";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.Multiple(() =>
		{
			//Assert.That(result.Error, Is.Null);
			Assert.That(result.Matches!, Has.Count.EqualTo(2));
			Assert.That(result.Matches![0].Value!["title"]!.GetValue<string>(), Is.EqualTo("Sayings of the Century"));
			Assert.That(result.Matches[1].Value!["title"]!.GetValue<string>(), Is.EqualTo("Moby Dick"));
			// the parens have been deemed unnecessary now.
			//Assert.That(path.ToString(), Is.EqualTo(input));
		});
	}

	[Test]
	public void Example10()
	{
		var input = "$..*";
		var path = JsonPath.Parse(input);

		var result = path.Evaluate(_instance);

		Assert.Multiple(() =>
		{
			//Assert.That(result.Error, Is.Null);
			Assert.That(result.Matches!, Has.Count.EqualTo(27));
			Assert.That(path.ToString(), Is.EqualTo(input));
		});
	}
}