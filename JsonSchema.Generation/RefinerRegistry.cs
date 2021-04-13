using System.Collections.Generic;
using System.Linq;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Tracks the available refiners.
	/// </summary>
	public static class RefinerRegistry
	{
		private static readonly List<ISchemaRefiner> _refiners = new List<ISchemaRefiner>();

		/// <summary>
		/// Registers a new refiner.
		/// </summary>
		/// <param name="refiner">The refiner.</param>
		public static void Register(ISchemaRefiner refiner)
		{
			_refiners.Insert(0, refiner);
		}

		internal static IEnumerable<ISchemaRefiner> Get(SchemaGeneratorContext context)
		{
			return _refiners.Where(g => g.ShouldRun(context));
		}
	}
}