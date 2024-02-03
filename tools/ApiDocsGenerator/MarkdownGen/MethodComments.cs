namespace ApiDocsGenerator.MarkdownGen;

/// <summary>
///     Method, operator and constructor comments
/// </summary>
public class MethodComments : CommonComments
{
	/// <summary>
	///     "param" comments of the method. Each item in the list is the tuple
	///     where Item1 is the "name" of the parameter in XML file and
	///     Item2 is the body of the comment.
	/// </summary>
	public List<(string Name, string Text)> Parameters { get; set; } = [];

	/// <summary>
	///     "returns" comment of the method.
	/// </summary>
	public string? Returns { get; set; }

	/// <summary>
	///     "response" comments of the method. The list contains tuples where
	///     Item1 is the "code" of the response and
	///     Item1 is the body of the comment.
	/// </summary>
	public List<(string Code, string Text)> Responses { get; set; } = [];

	/// <summary>
	///     "typeparam" comments of the method. Each item in the list is the tuple
	///     where Item1 is the "name" of the parameter in XML file and
	///     Item2 is the body of the comment.
	/// </summary>
	public List<(string Name, string Text)> TypeParameters { get; set; } = [];
}
