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

		internal Format(){}
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
	}
}