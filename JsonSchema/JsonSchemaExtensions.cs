using System.Text.Json;

namespace Json.Schema;

/// <summary>
/// Some extensions for <see cref="JsonSchema"/>
/// </summary>
public static class JsonSchemaExtensions
{
	/// <summary>
	/// Extends <see cref="JsonSchema.Validate(JsonElement,ValidationOptions)"/> to take <see cref="JsonDocument"/>.
	/// </summary>
	/// <param name="jsonSchema">The schema.</param>
	/// <param name="jsonDocument">Document to be validated.</param>
	/// <param name="options">The options to use for this validation.</param>
	/// <returns>A <see cref="ValidationResults"/> that provides the outcome of the validation.</returns>
	public static ValidationResults Validate(this JsonSchema jsonSchema, JsonDocument jsonDocument, ValidationOptions? options = null)
	{
		return jsonSchema.Validate(jsonDocument.RootElement, options);
	}

	/// <summary>
	/// Extends <see cref="JsonSchema.Validate(JsonElement,ValidationOptions)"/> to take <see cref="JsonDocument"/>.
	/// </summary>
	/// <param name="jsonSchema">The schema.</param>
	/// <param name="jsonString">A string containing a JSON instance to be validated.</param>
	/// <param name="options">The options to use for this validation.</param>
	/// <returns>A <see cref="ValidationResults"/> that provides the outcome of the validation.</returns>
	public static ValidationResults Validate(this JsonSchema jsonSchema, string jsonString, ValidationOptions? options = null)
	{
		using var jsonDocument = JsonDocument.Parse(jsonString);
		return jsonSchema.Validate(jsonDocument, options);
	}
}