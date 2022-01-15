using System;
using System.Linq;
using Json.More;

namespace Json.Schema.DataGeneration
{
	public class ArrayGenerator : IDataGenerator
	{
		public static ArrayGenerator Instance { get; } = new ArrayGenerator();

		public static uint DefaultMinItems { get; set; } = 0;
		public static uint DefaultMaxItems { get; set; } = 10;

		private ArrayGenerator() { }

		public SchemaValueType Type => SchemaValueType.Array;

		public GenerationResult Generate(JsonSchema schema)
		{
			var itemsKeyword = schema.Keywords.OfType<ItemsKeyword>().FirstOrDefault();
			if (itemsKeyword?.ArraySchemas != null)
			{
				throw new NotImplementedException();
			}

			var itemsSchema = itemsKeyword?.SingleSchema ?? true;
			var minItems = schema.Keywords?.OfType<MinItemsKeyword>().FirstOrDefault()?.Value ?? DefaultMinItems;
			var maxItems = schema.Keywords?.OfType<MaxItemsKeyword>().FirstOrDefault()?.Value ?? DefaultMaxItems;
			var itemCount = (int) JsonSchemaExtensions.Randomizer.UInt(minItems, maxItems);

			var itemGenerationResults = Enumerable.Range(0, itemCount).Select(x => itemsSchema.GenerateData()).ToArray();
			return itemGenerationResults.All(x => x.IsSuccess)
				? GenerationResult.Success(itemGenerationResults.Select(x => x.Result).AsJsonElement())
				: GenerationResult.Fail(itemGenerationResults);
		}
	}
}