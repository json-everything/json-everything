namespace Json.Schema
{
	/// <summary>
	/// Defines basic functionality for schema keywords.
	/// </summary>
	public interface IJsonSchemaKeyword
	{
		/// <summary>
		/// Provides validation for the keyword.
		/// </summary>
		/// <param name="context">Contextual details for the validation process.</param>
		void Validate(ValidationContext context);
	}
}