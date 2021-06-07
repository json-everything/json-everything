using System.Collections.Generic;

namespace Json.Path
{
	internal abstract class SelectorBase : ISelector
	{
		public virtual void Evaluate(EvaluationContext context)
		{
			var toProcess = new List<PathMatch>(context.Current);
			context.Current.Clear();
			foreach (var match in toProcess)
			{
				context.Current.AddRange(ProcessMatch(match));
			}
		}

		protected abstract IEnumerable<PathMatch> ProcessMatch(PathMatch match);
	}
}