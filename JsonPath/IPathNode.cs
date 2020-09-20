namespace Json.Path
{
	internal interface IPathNode
	{
		void Evaluate(EvaluationContext context);
	}
}