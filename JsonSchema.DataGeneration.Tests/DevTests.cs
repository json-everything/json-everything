using System;
using Json.More;
using NUnit.Framework;

namespace Json.Schema.DataGeneration.Tests
{
	public class DevTests
	{
		[Test]
		public void Test()
		{
			JsonSchema schema = new JsonSchemaBuilder()
				.Type(SchemaValueType.Integer)
				.Minimum(10000)
				.Maximum(20000)
				.MultipleOf(0.3m);

			var result = schema.GenerateData();

			Console.WriteLine(result.Result.ToJsonString());
		}
	}
}