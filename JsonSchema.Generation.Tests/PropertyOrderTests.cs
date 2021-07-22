using System.Linq;
using NUnit.Framework;

namespace Json.Schema.Generation.Tests
{
	public class PropertyOrderTests
	{
		private class SpecifiedOrder
		{
			public int Second { get; set; }
			public int First { get; set; }
		}

		private class SpecifiedOrderDerived : SpecifiedOrder
		{
			public int Third { get; set; }
		}

		[Test]
		public void PropertiesAsDeclared()
		{
			var config = new SchemaGeneratorConfiguration
			{
				PropertyOrder = PropertyOrder.AsDeclared
			};

			JsonSchema schema = new JsonSchemaBuilder()
				.FromType<SpecifiedOrder>(config);

			var properties = schema.Keywords.OfType<PropertiesKeyword>().Single();

			Assert.AreEqual(nameof(SpecifiedOrder.Second), properties.Properties.Keys.First());
			Assert.AreEqual(nameof(SpecifiedOrder.First), properties.Properties.Keys.Last());
		}

		[Test]
		public void PropertiesAsDeclaredByType()
		{
			var config = new SchemaGeneratorConfiguration
			{
				PropertyOrder = PropertyOrder.AsDeclared
			};

			JsonSchema schema = new JsonSchemaBuilder()
				.FromType<SpecifiedOrderDerived>(config);

			var properties = schema.Keywords.OfType<PropertiesKeyword>().Single();

			Assert.AreEqual(nameof(SpecifiedOrder.Second), properties.Properties.Keys.ElementAt(0));
			Assert.AreEqual(nameof(SpecifiedOrder.First), properties.Properties.Keys.ElementAt(1));
			Assert.AreEqual(nameof(SpecifiedOrderDerived.Third), properties.Properties.Keys.ElementAt(2));
		}

		[Test]
		public void PropertiesByName()
		{
			var config = new SchemaGeneratorConfiguration
			{
				PropertyOrder = PropertyOrder.ByName
			};

			JsonSchema schema = new JsonSchemaBuilder()
				.FromType<SpecifiedOrder>(config);

			var properties = schema.Keywords.OfType<PropertiesKeyword>().Single();

			Assert.AreEqual(nameof(SpecifiedOrder.First), properties.Properties.Keys.First());
			Assert.AreEqual(nameof(SpecifiedOrder.Second), properties.Properties.Keys.Last());
		}
	}
}
