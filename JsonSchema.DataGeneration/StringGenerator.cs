using System;

namespace Json.Schema.DataGeneration
{
	public class StringGenerator : IDataGenerator
	{
		public static StringGenerator Instance { get; } = new StringGenerator();

		private StringGenerator()
		{
		}

		public SchemaValueType Type => SchemaValueType.String;

		public GenerationResult Generate(JsonSchema schema)
		{
			throw new NotImplementedException();
		}
	}
}