namespace Json.Path
{
	internal interface ISelector
	{
		void Evaluate(EvaluationContext context);
	}
}