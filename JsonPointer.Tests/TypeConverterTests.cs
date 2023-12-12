using System.ComponentModel;
using NUnit.Framework;

namespace Json.Pointer.Tests
{
  public class TypeConverterTests
  {

		[Test]
		public void ConvertFromString()
		{
			var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPointer));
			var pointer = typeConverter.ConvertFromInvariantString("/foo") as JsonPointer;

			Assert.IsNotNull(pointer);
			Assert.AreEqual("/foo", pointer!.ToString());
		}

		[Test]
		public void ConvertToString()
		{
			var pointer = JsonPointer.Parse("/foo");
			var typeConverter = TypeDescriptor.GetConverter(typeof(JsonPointer));
			var pointerString = typeConverter.ConvertToInvariantString(pointer);

			Assert.AreEqual("/foo", pointerString);
		}

  }
}
