using System.Linq;
using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace Json.Path.Tests
{
	public class ParsedGoessnerTests
	{
		private readonly JsonElement _instance;

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

			_instance = model.ToJsonDocument(new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
				IgnoreNullValues = true
			}).RootElement;
		}

		[Test]
		public void GrammarExample()
		{
			var input = "$.store.book[0].title";
			var path = JsonPath.Parse(input);

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(1, result.Matches.Count);
			Assert.AreEqual("Sayings of the Century", result.Matches[0].Value.GetString());
			Assert.AreEqual(input, path.ToString());

		}

		[Test]
		public void Example1()
		{
			var input = "$.store.book[*].author";
			var path = JsonPath.Parse(input);

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(4, result.Matches.Count);
			Assert.AreEqual("Nigel Rees", result.Matches[0].Value.GetString());
			Assert.AreEqual("Evelyn Waugh", result.Matches[1].Value.GetString());
			Assert.AreEqual("Herman Melville", result.Matches[2].Value.GetString());
			Assert.AreEqual("J. R. R. Tolkien", result.Matches[3].Value.GetString());
			Assert.AreEqual(input, path.ToString());
		}

		[Test]
		public void Example2()
		{
			var input = "$..author";
			var path = JsonPath.Parse(input);

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(4, result.Matches.Count);
			Assert.AreEqual("Nigel Rees", result.Matches[0].Value.GetString());
			Assert.AreEqual("Evelyn Waugh", result.Matches[1].Value.GetString());
			Assert.AreEqual("Herman Melville", result.Matches[2].Value.GetString());
			Assert.AreEqual("J. R. R. Tolkien", result.Matches[3].Value.GetString());
			Assert.AreEqual(input, path.ToString());
		}

		[Test]
		public void Example3()
		{
			var input = "$.store.*";
			var path = JsonPath.Parse(input);

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(2, result.Matches.Count);
			Assert.AreEqual(4, result.Matches[0].Value.EnumerateArray().Count());
			Assert.AreEqual(2, result.Matches[1].Value.EnumerateObject().Count());
			Assert.AreEqual(input, path.ToString());
		}

		[Test]
		public void Example4()
		{
			var input = "$.store..price";
			var path = JsonPath.Parse(input);

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(5, result.Matches.Count);
			Assert.AreEqual(8.95m, result.Matches[0].Value.GetDecimal());
			Assert.AreEqual(12.99m, result.Matches[1].Value.GetDecimal());
			Assert.AreEqual(8.99m, result.Matches[2].Value.GetDecimal());
			Assert.AreEqual(22.99m, result.Matches[3].Value.GetDecimal());
			Assert.AreEqual(19.95m, result.Matches[4].Value.GetDecimal());
			Assert.AreEqual(input, path.ToString());
		}

		[Test]
		public void Example5()
		{
			var input = "$..book[2]";
			var path = JsonPath.Parse(input);

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(1, result.Matches.Count);
			Assert.AreEqual("Moby Dick", result.Matches[0].Value.GetProperty("title").GetString());
			Assert.AreEqual(input, path.ToString());
		}

		[Test]
		public void Example6a()
		{
			var input = "$..book[(@.length-1)]";
			var path = JsonPath.Parse(input);

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(1, result.Matches.Count);
			Assert.AreEqual("The Lord of the Rings", result.Matches[0].Value.GetProperty("title").GetString());
			Assert.AreEqual(input, path.ToString());
		}

		[Test]
		public void Example6b()
		{
			var input = "$..book[-1:]";
			var path = JsonPath.Parse(input);

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(1, result.Matches.Count);
			Assert.AreEqual("The Lord of the Rings", result.Matches[0].Value.GetProperty("title").GetString());
			Assert.AreEqual(input, path.ToString());
		}

		[Test]
		public void Example7a()
		{
			var input = "$..book[0,1]";
			var path = JsonPath.Parse(input);

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(2, result.Matches.Count);
			Assert.AreEqual("Sayings of the Century", result.Matches[0].Value.GetProperty("title").GetString());
			Assert.AreEqual("Sword of Honour", result.Matches[1].Value.GetProperty("title").GetString());
			Assert.AreEqual(input, path.ToString());
		}

		[Test]
		public void Example7b()
		{
			var input = "$..book[:2]";
			var path = JsonPath.Parse(input);

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(2, result.Matches.Count);
			Assert.AreEqual("Sayings of the Century", result.Matches[0].Value.GetProperty("title").GetString());
			Assert.AreEqual("Sword of Honour", result.Matches[1].Value.GetProperty("title").GetString());
			Assert.AreEqual(input, path.ToString());
		}

		[Test]
		public void Example8()
		{
			var input = "$..book[?(@.isbn)]";
			var path = JsonPath.Parse(input);

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(2, result.Matches.Count);
			Assert.AreEqual("Moby Dick", result.Matches[0].Value.GetProperty("title").GetString());
			Assert.AreEqual("The Lord of the Rings", result.Matches[1].Value.GetProperty("title").GetString());
			Assert.AreEqual(input, path.ToString());
		}

		[Test]
		public void Example9()
		{
			var input = "$..book[?(@.price<10)]";
			var path = JsonPath.Parse(input);

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(2, result.Matches.Count);
			Assert.AreEqual("Sayings of the Century", result.Matches[0].Value.GetProperty("title").GetString());
			Assert.AreEqual("Moby Dick", result.Matches[1].Value.GetProperty("title").GetString());
			Assert.AreEqual(input, path.ToString());
		}

		[Test]
		public void Example10()
		{
			var input = "$..*";
			var path = JsonPath.Parse(input);

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(27, result.Matches.Count);
			Assert.AreEqual(input, path.ToString());
		}
	}
}