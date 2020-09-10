using System.Linq;
using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace JsonPath.Tests
{
	public class ParsedGoessnerTests
	{
		//{ "store": {
		//		"book": [
		//		{ "category": "reference",
		//			"author": "Nigel Rees",
		//			"title": "Sayings of the Century",
		//			"price": 8.95
		//		},
		//		{ "category": "fiction",
		//			"author": "Evelyn Waugh",
		//			"title": "Sword of Honour",
		//			"price": 12.99
		//		},
		//		{
		//			"category": "fiction",
		//			"author": "Herman Melville",
		//			"title": "Moby Dick",
		//			"isbn": "0-553-21311-3",
		//			"price": 8.99
		//		},
		//		{
		//			"category": "fiction",
		//			"author": "J. R. R. Tolkien",
		//			"title": "The Lord of the Rings",
		//			"isbn": "0-395-19395-8",
		//			"price": 22.99
		//		}
		//		],
		//		"bicycle": {
		//			"color": "red",
		//			"price": 19.95
		//		}
		//	}
		//}
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
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}).RootElement;
		}

		// $.store.book[0].title
		[Test]
		public void GrammarExample()
		{
			var path =JsonPath.Parse("$.store.book[0].title");

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(1, result.Matches.Count);
			Assert.AreEqual("Sayings of the Century", result.Matches[0].Value.GetString());
		}

		// $.store.book[*].author
		[Test]
		public void Example1()
		{
			var path = JsonPath.Parse("$.store.book[*].author");

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(4, result.Matches.Count);
			Assert.AreEqual("Nigel Rees", result.Matches[0].Value.GetString());
			Assert.AreEqual("Evelyn Waugh", result.Matches[1].Value.GetString());
			Assert.AreEqual("Herman Melville", result.Matches[2].Value.GetString());
			Assert.AreEqual("J. R. R. Tolkien", result.Matches[3].Value.GetString());
		}

		// $..author
		[Test]
		public void Example2()
		{
			var path = JsonPath.Parse("$..author");

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(4, result.Matches.Count);
			Assert.AreEqual("Nigel Rees", result.Matches[0].Value.GetString());
			Assert.AreEqual("Evelyn Waugh", result.Matches[1].Value.GetString());
			Assert.AreEqual("Herman Melville", result.Matches[2].Value.GetString());
			Assert.AreEqual("J. R. R. Tolkien", result.Matches[3].Value.GetString());
		}

		// $.store.*
		[Test]
		public void Example3()
		{
			var path = JsonPath.Parse("$.store.*");

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(2, result.Matches.Count);
			Assert.AreEqual(4, result.Matches[0].Value.EnumerateArray().Count());
			Assert.AreEqual(2, result.Matches[1].Value.EnumerateObject().Count());
		}

		// $.store..price
		[Test]
		public void Example4()
		{
			var path = JsonPath.Parse("$.store..price");

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(5, result.Matches.Count);
			Assert.AreEqual(8.95m, result.Matches[0].Value.GetDecimal());
			Assert.AreEqual(12.99m, result.Matches[1].Value.GetDecimal());
			Assert.AreEqual(8.99m, result.Matches[2].Value.GetDecimal());
			Assert.AreEqual(22.99m, result.Matches[3].Value.GetDecimal());
			Assert.AreEqual(19.95m, result.Matches[4].Value.GetDecimal());
		}

		// $..book[2]
		[Test]
		public void Example5()
		{
			var path = JsonPath.Parse("$..book[2]");

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(1, result.Matches.Count);
			Assert.AreEqual("Moby Dick", result.Matches[0].Value.GetProperty("title").GetString());
		}

		// $..book[-1:]
		[Test]
		public void Example6b()
		{
			var path = JsonPath.Parse("$..book[-1:]");

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(1, result.Matches.Count);
			Assert.AreEqual("The Lord of the Rings", result.Matches[0].Value.GetProperty("title").GetString());
		}

		// $..book[0,1]
		[Test]
		public void Example7a()
		{
			var path = JsonPath.Parse("$..book[0,1]");

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(2, result.Matches.Count);
			Assert.AreEqual("Sayings of the Century", result.Matches[0].Value.GetProperty("title").GetString());
			Assert.AreEqual("Sword of Honour", result.Matches[1].Value.GetProperty("title").GetString());
		}

		// $..book[:2]
		[Test]
		public void Example7b()
		{
			var path = JsonPath.Parse("$..book[:2]");

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(2, result.Matches.Count);
			Assert.AreEqual("Sayings of the Century", result.Matches[0].Value.GetProperty("title").GetString());
			Assert.AreEqual("Sword of Honour", result.Matches[1].Value.GetProperty("title").GetString());
		}

		// $..*
		[Test]
		public void Example10()
		{
			var path = JsonPath.Parse("$..*");

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(29, result.Matches.Count);
		}
	}
}