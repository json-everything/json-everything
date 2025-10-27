using System.ComponentModel;
using System.Linq;
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

		[Test]
		public void ConvertLogicalExpressionToJsonPath()
		{
			var path = JsonPath.Parse("$..[?(@['$ref'] == '#/components/schemas/Metadata' && @.anyOf)]['$ref']");
			var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPath));
			var result = $"{(path.Scope is PathScope.Global ? "$" : "@")}{string.Concat(path.Segments[..^1].Select(static s => s.ToString()))}";

			Assert.That(result, Is.EqualTo("$..[?@['$ref']==\"#/components/schemas/Metadata\"&&@.anyOf]"));
		}

	}
}
