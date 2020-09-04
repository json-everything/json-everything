namespace JsonPath
{
	public interface IPathNode
	{
		void Evaluate(EvaluationContext context);
	}
}