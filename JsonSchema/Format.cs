using System.Text.Json;

namespace Json.Schema
{
	/// <summary>
	/// Represents a format.
	/// </summary>
	public class Format
	{
		/// <summary>
		/// The format key.
		/// </summary>
		public string Key { get; }

#pragma warning disable 8618
		internal Format(){}
#pragma warning restore 8618
		/// <summary>
		/// Creates a new <see cref="Format"/>.
		/// </summary>
		/// <param name="key">The format key.</param>
		public Format(string key)
		{
			Key = key;
		}

		/// <summary>
		/// Validates an instance against a format.
		/// </summary>
		/// <param name="element">The element to validate.</param>
		/// <returns><code>true</code>.  Override to return another value.</returns>
		public virtual bool Validate(JsonElement element)
		{
			return true;
		}

		/// <summary>
		/// Validates an instance against a format and provides an error message.
		/// </summary>
		/// <param name="element">The element to validate.</param>
		/// <param name="errorMessage">An error message.</param>
		/// <returns><code>true</code>.  Override to return another value.</returns>
		public virtual bool Validate(JsonElement element, out string? errorMessage)
		{
			errorMessage = null;
			return true;
		}
	}
}