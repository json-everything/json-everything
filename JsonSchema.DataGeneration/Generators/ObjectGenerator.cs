using System.Collections.Generic;
using System.Text.Json;
using Json.More;

namespace Json.Schema.DataGeneration.Generators
{
	internal class ObjectGenerator : IDataGenerator
	{
		public static ObjectGenerator Instance { get; } = new ObjectGenerator();

		private ObjectGenerator()
		{
		}

		public SchemaValueType Type => SchemaValueType.Object;

		public GenerationResult Generate(RequirementsContext context)
		{
			return GenerationResult.Success(new Dictionary<string, JsonElement>().AsJsonElement());
		}
	}
}