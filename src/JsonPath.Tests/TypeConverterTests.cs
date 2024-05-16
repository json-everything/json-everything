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

			Assert.That(path, Is.Not.Null);
			Assert.That(path!.ToString(), Is.EqualTo("$.store.book[*].author"));
		}

		[Test]
		public void ConvertToString()
		{
			var path = JsonPath.Parse("$.store.book[*].author");
			var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPath));
			var pathString = typeConverter.ConvertToInvariantString(path);

			Assert.That(pathString, Is.EqualTo("$.store.book[*].author"));
		}

		[Test]
		public void ConvertFromJsonPath()
		{
			var path = JsonPath.Parse("$.store.book[*].author");
			var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPath));
			var path2 = typeConverter.ConvertFrom(path) as JsonPath;

			Assert.That(path2, Is.Not.Null);
			Assert.That(path2, Is.Not.SameAs(path));
			Assert.That(path2!.ToString(), Is.EqualTo("$.store.book[*].author"));
		}

		[Test]
		public void ConvertToJsonPath()
		{
			var path = JsonPath.Parse("$.store.book[*].author");
			var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPath));
			var path2 = typeConverter.ConvertTo(path, typeof(JsonPath)) as JsonPath;

			Assert.That(path2, Is.Not.Null);
			Assert.That(path2, Is.Not.SameAs(path));
			Assert.That(path2!.ToString(), Is.EqualTo("$.store.book[*].author"));
		}

	}
}
