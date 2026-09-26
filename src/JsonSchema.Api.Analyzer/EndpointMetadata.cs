using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Microsoft.CodeAnalysis;

namespace Json.Schema.Api.Analyzer;

/// <summary>
/// The parts of a member's XML documentation comment that describe an operation.
/// </summary>
/// <remarks>
/// The compiler only parses documentation comments when the project generates a
/// documentation file, so a project without `GenerateDocumentationFile` yields nothing here.
/// </remarks>
internal sealed class DocComment
{
	/// <summary>
	/// Gets the `summary` text.
	/// </summary>
	public string? Summary { get; private set; }

	/// <summary>
	/// Gets the `remarks` text.
	/// </summary>
	public string? Remarks { get; private set; }

	/// <summary>
	/// Gets the `returns` text.
	/// </summary>
	public string? Returns { get; private set; }

	/// <summary>
	/// Gets the `param` texts, keyed by parameter name.
	/// </summary>
	public Dictionary<string, string> Parameters { get; } = new();

	/// <summary>
	/// Gets the `response` texts, keyed by status code.
	/// </summary>
	public Dictionary<int, string> Responses { get; } = new();

	/// <summary>
	/// Reads the documentation comment on a symbol.
	/// </summary>
	/// <param name="symbol">The symbol.</param>
	/// <returns>The comment, or null when the symbol has none.</returns>
	public static DocComment? Read(ISymbol symbol)
	{
		var xml = symbol.GetDocumentationCommentXml();
		if (string.IsNullOrWhiteSpace(xml)) return null;

		XElement root;
		try
		{
			root = XElement.Parse("<root>" + xml + "</root>");
		}
		catch (XmlException)
		{
			return null;
		}

		var container = root.Elements().FirstOrDefault(x => x.Name.LocalName is "member" or "doc") ?? root;

		var comment = new DocComment();

		foreach (var element in container.Elements())
		{
			var text = Flatten(element);
			if (text is null) continue;

			switch (element.Name.LocalName)
			{
				case "summary":
					comment.Summary = text;
					break;
				case "remarks":
					comment.Remarks = text;
					break;
				case "returns":
					comment.Returns = text;
					break;
				case "param":
					var name = element.Attribute("name")?.Value;
					if (!string.IsNullOrEmpty(name))
						comment.Parameters[name!] = text;
					break;
				case "response":
					if (int.TryParse(element.Attribute("code")?.Value, out var code))
						comment.Responses[code] = text;
					break;
			}
		}

		return comment;
	}

	/// <summary>
	/// Reduces an element to plain text: inline references become their names, paragraphs
	/// are separated by a blank line, and the source's line wrapping is removed.
	/// </summary>
	private static string? Flatten(XElement element)
	{
		var sb = new StringBuilder();
		foreach (var node in element.Nodes())
		{
			Append(sb, node);
		}

		var text = sb.ToString().Replace("\r", string.Empty);
		text = Regex.Replace(text, @"[ \t]*\n[ \t]*", "\n");
		text = Regex.Replace(text, @"[ \t]{2,}", " ");
		text = Regex.Replace(text, @"\n{3,}", "\n\n");
		text = Regex.Replace(text, @"(?<!\n)\n(?!\n)", " ");
		text = text.Trim();

		return text.Length == 0 ? null : text;
	}

	private static void Append(StringBuilder sb, XNode node)
	{
		if (node is XText text)
		{
			sb.Append(text.Value);
			return;
		}

		if (node is not XElement element) return;

		switch (element.Name.LocalName)
		{
			case "see":
			case "seealso":
				if (element.Nodes().Any())
					AppendChildren(sb, element);
				else
					sb.Append(ReferenceName(
						element.Attribute("cref")?.Value ??
						element.Attribute("langword")?.Value ??
						element.Attribute("href")?.Value));
				break;
			case "paramref":
			case "typeparamref":
				sb.Append(element.Attribute("name")?.Value);
				break;
			case "para":
				sb.Append("\n\n");
				AppendChildren(sb, element);
				sb.Append("\n\n");
				break;
			default:
				AppendChildren(sb, element);
				break;
		}
	}

	private static void AppendChildren(StringBuilder sb, XElement element)
	{
		foreach (var child in element.Nodes())
		{
			Append(sb, child);
		}
	}

	/// <summary>
	/// Reduces a documentation ID such as `T:System.String` or `M:Ns.Type.Method(System.Int32)`
	/// to its simple name.
	/// </summary>
	private static string ReferenceName(string? reference)
	{
		if (string.IsNullOrEmpty(reference)) return string.Empty;

		var value = reference!;

		if (value.Length > 2 && value[1] == ':')
			value = value.Substring(2);

		var paren = value.IndexOf('(');
		if (paren >= 0)
			value = value.Substring(0, paren);

		var dot = value.LastIndexOf('.');
		if (dot >= 0)
			value = value.Substring(dot + 1);

		var arity = value.IndexOf('`');
		if (arity >= 0)
			value = value.Substring(0, arity);

		return value;
	}
}

/// <summary>
/// Applies descriptive metadata to a discovered operation.
/// </summary>
/// <remarks>
/// ASP.NET's own metadata (`[EndpointSummary]`, `[EndpointDescription]`, `[Tags]`) is read
/// first; a documentation comment then fills whatever those left unset.
/// </remarks>
internal static class EndpointMetadata
{
	/// <summary>
	/// Applies the metadata attributes declared on a symbol.
	/// </summary>
	/// <param name="endpoint">The operation.</param>
	/// <param name="symbol">The controller, action, or handler.</param>
	public static void ApplyAttributes(EndpointInfo endpoint, ISymbol symbol)
	{
		foreach (var attribute in symbol.GetAttributes())
		{
			switch (attribute.AttributeClass?.Name)
			{
				case "EndpointSummaryAttribute":
					endpoint.Summary = StringArgument(attribute) ?? endpoint.Summary;
					break;
				case "EndpointDescriptionAttribute":
					endpoint.Description = StringArgument(attribute) ?? endpoint.Description;
					break;
				case "TagsAttribute":
					AddTags(endpoint, StringArrayArgument(attribute));
					break;
			}
		}
	}

	/// <summary>
	/// Applies a documentation comment, filling what the attributes left unset.
	/// </summary>
	/// <param name="endpoint">The operation.</param>
	/// <param name="comment">The comment, or null.</param>
	public static void ApplyDocComment(EndpointInfo endpoint, DocComment? comment)
	{
		if (comment is null) return;

		endpoint.Summary ??= comment.Summary;
		endpoint.Description ??= comment.Remarks;

		foreach (var parameter in endpoint.Parameters)
		{
			if (parameter.Description is null && comment.Parameters.TryGetValue(parameter.Name, out var text))
				parameter.Description = text;
		}

		if (endpoint.RequestBodyParameterName is not null &&
			endpoint.RequestBodyDescription is null &&
			comment.Parameters.TryGetValue(endpoint.RequestBodyParameterName, out var bodyText))
		{
			endpoint.RequestBodyDescription = bodyText;
		}

		if (comment.Returns is not null)
		{
			var success = endpoint.Responses.FirstOrDefault(x => x.StatusCode == 200);
			if (success is not null)
				success.Description = comment.Returns;
		}

		// `<response code="404">` describes a response the handler may not declare anywhere
		// else, so it adds one where none exists and re-describes one where it does.
		foreach (var kvp in comment.Responses)
		{
			var existing = endpoint.Responses.FirstOrDefault(x => x.StatusCode == kvp.Key);
			if (existing is not null)
				existing.Description = kvp.Value;
			else
				endpoint.Responses.Add(new EndpointResponseInfo { StatusCode = kvp.Key, Description = kvp.Value });
		}
	}

	/// <summary>
	/// Adds tags, ignoring any already present.
	/// </summary>
	public static void AddTags(EndpointInfo endpoint, IEnumerable<string> tags)
	{
		foreach (var tag in tags)
		{
			if (!endpoint.Tags.Contains(tag))
				endpoint.Tags.Add(tag);
		}
	}

	private static string? StringArgument(AttributeData attribute) =>
		attribute.ConstructorArguments.Length > 0
			? attribute.ConstructorArguments[0].Value as string
			: null;

	private static IEnumerable<string> StringArrayArgument(AttributeData attribute)
	{
		if (attribute.ConstructorArguments.Length == 0) return [];

		// The single `params string[]` parameter arrives as one array argument.
		return attribute.ConstructorArguments[0].Values
			.Select(x => x.Value as string)
			.Where(x => !string.IsNullOrWhiteSpace(x))
			.Select(x => x!);
	}
}
