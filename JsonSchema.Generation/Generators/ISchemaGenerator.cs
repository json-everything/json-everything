using System;

namespace Json.Schema.Generation.Generators
{
	/// <summary>
	/// Defines a generator.
	/// </summary>
	/// <remarks>
	///	Generators are the first stage of schema generation.  These will add keyword intents
	/// to the context, which then are translated into keywords after optimization.
	///
	/// Implementations MUST also override <see cref="object.GetHashCode()"/>
	/// </remarks>
	public interface ISchemaGenerator
	{
		/// <summary>
		/// Determines whether the generator can be used to generate a schema for this type.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <returns>`true` if the generator can be used; `false` otherwise.</returns>
		bool Handles(Type type);

		/// <summary>
		/// Processes the type and any attributes (present on the context), and adds
		/// intents to the context.
		/// </summary>
		/// <param name="context">The generation context.</param>
		void AddConstraints(SchemaGeneratorContext context);
	}
}