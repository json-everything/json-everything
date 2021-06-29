using Humanizer;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Declares a property naming method which is used to alter property names.
	/// </summary>
	/// <param name="input">The property name.</param>
	/// <returns>The altered property name.</returns>
	public delegate string PropertyNamingMethod(string input);

	/// <summary>
	/// Defines a set of predefined property naming methods.
	/// </summary>
	public static class PropertyNamingMethods
	{
		/// <summary>
		/// Makes no changes.  Properties are generated as they are declared in code.
		/// </summary>
		public static readonly PropertyNamingMethod AsDeclared = x => x;
		/// <summary>
		/// Updates property names to camel case (e.g. `camelCase`).
		/// </summary>
		public static readonly PropertyNamingMethod CamelCase = x => x.Camelize();
		/// <summary>
		/// Updates property names to pascal case (e.g. `PascalCase`).
		/// </summary>
		public static readonly PropertyNamingMethod PascalCase = x => x.Pascalize();
		/// <summary>
		/// Updates property names to snake case (e.g. `Snake_Case`).
		/// </summary>
		public static readonly PropertyNamingMethod SnakeCase = x => x.Underscore();
		/// <summary>
		/// Updates property names to lower snake case (e.g. `lower_snake_case`).
		/// </summary>
		public static readonly PropertyNamingMethod LowerSnakeCase = x => x.Underscore().ToLowerInvariant();
		/// <summary>
		/// Updates property names to upper snake case (e.g. `UPPER_SNAKE_CASE`).
		/// </summary>
		public static readonly PropertyNamingMethod UpperSnakeCase = x => x.Underscore().ToUpperInvariant();
		/// <summary>
		/// Updates property names to kebab case (e.g. `Kebab-Case`).
		/// </summary>
		public static readonly PropertyNamingMethod KebabCase = x => x.Kebaberize();
		/// <summary>
		/// Updates property names to lower kebab case (e.g. `UPPER-KEBAB-CASE`).
		/// </summary>
		public static readonly PropertyNamingMethod UpperKebabCase = x => x.Kebaberize().ToUpperInvariant();
	}
}