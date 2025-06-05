using System.ComponentModel;
using NUnit.Framework;

namespace Json.Pointer.Tests
{
  public class TypeConverterTests
  {

		[Test]
		public void ConvertFromString()
		{
			var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPointer_Old));
			var pointer = typeConverter.ConvertFromInvariantString("/foo");

			Assert.That(pointer, Is.InstanceOf<JsonPointer_Old>());
			Assert.That(pointer!.ToString(), Is.EqualTo("/foo"));
		}

		[Test]
		public void ConvertToString()
		{
			var pointer = JsonPointer_Old.Parse("/foo");
			var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPointer_Old));
			var pointerString = typeConverter.ConvertToInvariantString(pointer);

			Assert.That(pointerString, Is.EqualTo("/foo"));
		}

		[Test]
		public void ConvertFromJsonPointer()
		{
			var pointer = JsonPointer_Old.Parse("/foo");
			var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPointer_Old));
			var pointer2 = typeConverter.ConvertFrom(pointer);

			Assert.That(pointer2, Is.InstanceOf<JsonPointer_Old>());
			Assert.That(pointer2!.ToString(), Is.EqualTo("/foo"));
		}

		[Test]
		public void ConvertToJsonPointer()
		{
			var pointer = JsonPointer_Old.Parse("/foo");
			var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPointer_Old));
			var pointer2 = typeConverter.ConvertTo(pointer, typeof(JsonPointer_Old));

			Assert.That(pointer2, Is.InstanceOf<JsonPointer_Old>());
			Assert.That(pointer2!.ToString(), Is.EqualTo("/foo"));
		}

  }
}
