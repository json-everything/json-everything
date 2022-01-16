using System;
using Json.More;
using NUnit.Framework;

namespace Json.Schema.DataGeneration.Tests
{
	public static class TestHelpers
	{
		public static void Run(JsonSchema schema)
		{
			var result = schema.GenerateData();

			Assert.IsTrue(result.IsSuccess, "failed generation");
			Console.WriteLine(result.Result.ToJsonString());
			Assert.IsTrue(schema.Validate(result.Result).IsValid, "failed validation");
		}
	}
}