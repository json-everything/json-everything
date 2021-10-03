namespace Json.Path
{
	/// <summary>
	/// Provides options for path evaluation.
	/// </summary>
	public class PathEvaluationOptions
	{
		/// <summary>
		/// Provides options for experimental features.
		/// </summary>
		/// <remarks>
		/// Changes to this object will not be reflected in the version number.
		/// </remarks>
		public ExperimentalFeatures ExperimentalFeatures { get; } = new ExperimentalFeatures();
	}
}
