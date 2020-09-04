using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Json.More;
using NUnit.Framework;

namespace JsonPath.Tests
{
	public class GoessnerTests
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

		public GoessnerTests()
		{
			var model = new Repository
			{
				Store = new Store
				{
					Book = new[]
					{
						new Book
						{
							Title = "Nigel Rees"
						},
						new Book
						{
							Title = "Evelyn Waugh"
						},
						new Book
						{
							Title = "Herman Melville"
						},
						new Book
						{
							Title = "J. R. R. Tolkien"
						},
					}
				}
			};

			_instance = model.ToJsonDocument(new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			}).RootElement;
		}

		[Test]
		public void DynamicBuilder()
		{
			JsonPath path = new JsonPathBuilder()
				.Property("store")
				.Property("book")
				.Index(0)
				.Property("title");

			var result = path.Evaluate(_instance);

			Assert.IsNull(result.Error);
			Assert.AreEqual(1, result.Matches.Count);
			Assert.AreEqual("Nigel Rees", result.Matches[0].Value.GetString());
		}
	}

	public class Repository
	{
		public Store Store { get; set; }
	}

	public class Store
	{
		public IList<Book> Book { get; set; }
	}

	public class Book
	{
		public string Title { get; set; }
	}
}