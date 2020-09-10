using System.Collections.Generic;

namespace JsonPath
{
	public abstract class PathNodeBase : IPathNode
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