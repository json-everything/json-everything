namespace Json.Path
{
	public interface IPathNode
	{
		void Evaluate(EvaluationContext context);
	}
}