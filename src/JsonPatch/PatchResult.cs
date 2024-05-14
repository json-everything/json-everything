using System.Text.Json.Nodes;

namespace Json.Patch;

/// <summary>
/// Results for a JSON Patch application.
/// </summary>
public class PatchResult
{
	/// <summary>
	/// Gets the resulting JSON document.
	/// </summary>
	public JsonNode? Result { get; }
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
	#if NET5_0_OR_GREATER
	[System.Diagnostics.CodeAnalysis.MemberNotNullWhen(false, nameof(Error))]
	#endif
	public bool IsSuccess => Error == null;

	internal PatchResult(PatchContext context)
	{
		Result = context.Source;
		Error = context.Message;
		Operation = context.Index;
	}
}