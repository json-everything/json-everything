using System.Collections.Generic;

namespace Json.Schema;

public interface IRootCauseAnalyzer
{
	EvaluationResultsAnalysis? FindIssues(EvaluationResults root);
}

public abstract class EvaluationResultsAnalysis
{
}

public class FixAll : EvaluationResultsAnalysis
{
	public IEnumerable<EvaluationResultsAnalysis> Issues { get; set; }
}

public class FixAny : EvaluationResultsAnalysis
{
	public IEnumerable<EvaluationResultsAnalysis> Issues { get; set; }
}

public class FixThis : EvaluationResultsAnalysis
{
	public EvaluationResults Issue { get; set; }
}