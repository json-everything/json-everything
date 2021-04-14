namespace Json.Schema.Generation
{
	/// <summary>
	/// Describes a schema generation refiner.
	/// </summary>
	/// <remarks>
	/// Refiners run after attributes have been processed, before the
	/// schema itself is created.  This is used to add customization
	/// logic.
	/// </remarks>
	public interface ISchemaRefiner
	{
		/// <summary>
		/// Determines if the refiner should run.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		bool ShouldRun(SchemaGeneratorContext context);

		/// <summary>
		/// Runs the refiner.
		/// </summary>
		/// <param name="context"></param>
		void Run(SchemaGeneratorContext context);
	}
}