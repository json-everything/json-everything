using System.Text.Json;

namespace Json.Patch
{
	/// <summary>
	/// Results for a JSON Patch application.
	/// </summary>
	public class PatchResult
	{
		/// <summary>
		/// Gets the resulting JSON document.
		/// </summary>
		public JsonElement Result { get; }
		/// <summary>
		/// Gets any error that occurred.
		/// </summary>
		public string? Error { get; }
		/// <summary>
		/// Gets the last operation that was attempted.
		/// </summary>
		public int Operation { get; }

		/// <summary>
		/// Gets whether there was an error.
		/// </summary>
		public bool IsSuccess => Error == null;

		internal PatchResult(PatchContext context)
		{
			Result = context.Source.ToElement();
			Error = context.Message;
			Operation = context.Index;
		}
	}
}