using System.ComponentModel;
using NUnit.Framework;

namespace Json.Path.Tests
{
	public class TypeConverterTests
	{

		[Test]
		public void ConvertFromString()
		{
			var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPath));
			var path = typeConverter.ConvertFromInvariantString("$.store.book[*].author") as JsonPath;

			Assert.IsNotNull(path);
			Assert.AreEqual("$.store.book[*].author", path!.ToString());
		}

		[Test]
		public void ConvertToString()
		{
			var path = JsonPath.Parse("$.store.book[*].author");
			var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPath));
			var pathString = typeConverter.ConvertToInvariantString(path);

			Assert.AreEqual("$.store.book[*].author", pathString);
		}

		[Test]
		public void ConvertFromJsonPath()
		{
			var path = JsonPath.Parse("$.store.book[*].author");
			var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPath));
			var path2 = typeConverter.ConvertFrom(path) as JsonPath;

			Assert.IsNotNull(path2);
			Assert.AreNotSame(path, path2);
			Assert.AreEqual("$.store.book[*].author", path2!.ToString());
		}

		[Test]
		public void ConvertToJsonPath()
		{
			var path = JsonPath.Parse("$.store.book[*].author");
			var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPath));
			var path2 = typeConverter.ConvertTo(path, typeof(JsonPath)) as JsonPath;

			Assert.IsNotNull(path2);
			Assert.AreNotSame(path, path2);
			Assert.AreEqual("$.store.book[*].author", path2!.ToString());
		}

	}
}
