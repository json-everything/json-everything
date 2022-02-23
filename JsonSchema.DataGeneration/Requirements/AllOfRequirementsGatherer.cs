using System.Linq;
using JetBrains.Annotations;

namespace Json.Schema.DataGeneration.Requirements
{
[UsedImplicitly]
	internal class AllOfRequirementsGatherer : IRequirementsGatherer
	{
		public void AddRequirements(RequirementsContext context, JsonSchema schema)
		{
			var allOfKeyword = schema.Keywords?.OfType<AllOfKeyword>().FirstOrDefault();
			if (allOfKeyword == null) return;

			foreach (var subschema in allOfKeyword.Schemas)
			{
				// ReSharper disable once IdentifierTypo
				var subrequirement = subschema.GetRequirements();
				context.And(subrequirement);
			}
		}
	}
}