using System.Collections.Generic;

namespace Json.Path.Tests
{
	public class Repository
	{
		public Store Store { get; set; }
	}

	public class Store
	{
		public IList<Book> Book { get; set; }
		public Bicycle Bicycle { get; set; }
	}

	public class Bicycle
	{
		public string Color { get; set; }
		public decimal Price { get; set; }
	}

	public class Book
	{
		public string Author { get; set; }
		public string Category { get; set; }
		public string Title { get; set; }
		public string Isbn { get; set; }
		public decimal Price { get; set; }
	}

}
