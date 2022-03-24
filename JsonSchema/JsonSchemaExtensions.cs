using System.Text.Json;

namespace Json.Schema
{
	/// <summary>
	/// Some extensions for <see cref="JsonSchema"/>
	/// </summary>
	public static class JsonSchemaExtensions
	{
		/// <summary>
		/// Validate JsonDocument with jsonSchema
		/// </summary>
		/// <param name="jsonSchema">jsonSchema</param>
		/// <param name="jsonDocument">jsonDocument to be validated</param>
		/// <param name="validationOptions">validationOptions for validate</param>
		/// <returns>The validation result <see cref="ValidationResults"/> for the provided jsonDocument</returns>
		public static ValidationResults Validate(this JsonSchema jsonSchema, JsonDocument jsonDocument, ValidationOptions? validationOptions = null)
		{
			return jsonSchema.Validate(jsonDocument.RootElement, validationOptions);
		}

		/// <summary>
		/// Validate json string with jsonSchema
		/// </summary>
		/// <param name="jsonSchema">jsonSchema</param>
		/// <param name="jsonString">json string to be validated</param>
		/// <param name="validationOptions">validationOptions for validate</param>
		/// <returns>The validation result <see cref="ValidationResults"/> for the provided json string</returns>
		public static ValidationResults Validate(this JsonSchema jsonSchema, string jsonString, ValidationOptions? validationOptions = null)
		{
			using var jsonDocument = JsonDocument.Parse(jsonString);
			return jsonSchema.Validate(jsonDocument, validationOptions);
		}
	}
}