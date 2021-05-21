using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
