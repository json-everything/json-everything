using System;

namespace Json.Schema.DataGeneration.Generators
{
	internal class ObjectGenerator : IDataGenerator
	{
		public static ObjectGenerator Instance { get; } = new ObjectGenerator();

		private ObjectGenerator()
		{
		}

		public SchemaValueType Type => SchemaValueType.Object;

		public GenerationResult Generate(RequirementContext context)
		{
			throw new NotImplementedException();
		}
	}
}