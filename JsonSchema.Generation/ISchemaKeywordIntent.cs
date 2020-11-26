using Json.Schema.Generation.Generators;

namespace Json.Schema.Generation
{
	/// <summary>
	/// Describes the intent to create a keyword.
	/// </summary>
	/// <remarks>
	/// Because <see cref="JsonSchema"/> is immutable, the system cannot
	/// generate the schema directly as it needs to do some optimization
	/// first.  Keyword intents allow this.  They record all of the data
	/// needed by the keyword.  Application involves translating the
	/// intent into an actual keyword on the <see cref="JsonSchemaBuilder"/>
	/// using one of the fluent extension methods provided by
	/// <see cref="Schema.JsonSchemaBuilderExtensions"/>.  Custom intents
	/// will need to be applied from within custom <see cref="ISchemaGenerator"/>
	/// implementations.
	///
	/// Implementations MUST also override <see cref="object.GetHashCode()"/>
	/// </remarks>
	public interface ISchemaKeywordIntent
	{
		/// <summary>
		/// Applies the keyword to the <see cref="JsonSchemaBuilder"/>.
		/// </summary>
		/// <param name="builder">The builder.</param>
		void Apply(JsonSchemaBuilder builder);
	}
}