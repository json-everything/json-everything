using System.Linq;
using JetBrains.Annotations;

namespace Json.Schema.DataGeneration.Requirements
{
	[UsedImplicitly]
	internal class TypeRequirementsGatherer : IRequirementsGatherer
	{
		public void AddRequirements(RequirementsContext context, JsonSchema schema)
		{
			var typeKeyword = schema.Keywords?.OfType<TypeKeyword>().FirstOrDefault();
			if (typeKeyword == null) return;

			if (context.Type == null)
				context.Type = typeKeyword.Type;
			else
				context.Type &= typeKeyword.Type;
		}
	}
}