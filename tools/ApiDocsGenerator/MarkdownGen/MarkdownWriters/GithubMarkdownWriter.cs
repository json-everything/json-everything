using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Json.More;

namespace ApiDocsGenerator.MarkdownGen.MarkdownWriters;

public class GithubMarkdownWriter : IMarkdownWriter
{
	public string FormatName => "github";

	public string FullText => _allText.ToString();

	#region Basic writing

	private readonly StringBuilder _allText = new();
	private readonly JsonSerializerOptions _serializerOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

	public void AddFrontMatter(JsonObject data)
	{
		_allText.AppendLine("---");
		foreach (var kvp in data)
		{
			_allText.AppendLine($"{kvp.Key}: {kvp.Value.AsJsonString(_serializerOptions)}");
		}
		_allText.AppendLine("---");
	}

	public void Write(string? text)
	{
		if (text == null) return;
		_allText.AppendLine(text);
	}

	public void WriteLine(string? text)
	{
		if (text != null) _allText.AppendLine(text);
		_allText.AppendLine();
	}

	#endregion

	#region Formatted writing

	public void WriteH1(string text)
	{
		WriteLine("# " + EscapeSpecialChars(text));
	}

	public void WriteH2(string text)
	{
		WriteLine("## " + EscapeSpecialChars(text));
	}

	public void WriteH3(string text)
	{
		WriteLine("### " + EscapeSpecialChars(text));
	}

	public void WriteH4(string text)
	{
		WriteLine("#### " + EscapeSpecialChars(text));
	}

	public void WriteLink(string anchorName, string text)
	{
		_allText.Append(Link(anchorName, text));
	}

	public void WriteHeadingLink(string text)
	{
		_allText.Append(HeadingLink(text));
	}

	public void WriteAnchor(string anchorName)
	{
		_allText.Append($"<a name=\"{anchorName}\"></a>");
	}

	public void WriteHorizontalRule()
	{
		_allText.AppendLine();
		WriteLine("---");
	}

	public void WriteListItem(string text)
	{
		_allText.AppendLine("- " + EscapeSpecialChars(text));
	}

	public void WriteCodeBlock(string text)
	{
		_allText.AppendLine("```c#");
		_allText.AppendLine(text);
		_allText.AppendLine("```");
		_allText.AppendLine();
	}

	#endregion

	#region Tables

	public void WriteTableTitle(params string[] tableHeadings)
	{
		Write("| " + string.Join(" | ", tableHeadings) + " |");
		Write("|" + string.Join("|", tableHeadings.Select(_ => "---")) + "|");
	}

	public void WriteTableRow(params string?[] texts)
	{
		Write("| " + string.Join(" | ", texts.Select(EscapeSpecialText)) + " |");
	}

	public void WriteEndTable()
	{
		_allText.AppendLine();
	}

	#endregion

	#region Text formatting

	private string EscapeSpecialText(string? text)
	{
		if (text == null) return "";
		text = ResolveTag(text, "paramref", "name");
		return EscapeSpecialChars(text);
	}

	private string ResolveTag(string text, string tagName, string attributeName)
	{
		var regex = new Regex("<" + tagName + "( +)" + attributeName + "( *)=( *)\"(.*?)\"( *)/>");
		for (; ; )
		{
			var match = regex.Match(text);
			if (!match.Success) return text;

			var attributeValue = match.Groups[4].Value;
			text = text[..match.Index] + Bold(attributeValue) + text[(match.Index + match.Length)..];
		}
	}

	private static string EscapeSpecialChars(string? text)
	{
		if (text == null) return "";
		text = text.Replace("<", "\\<");
		text = text.Replace(">", "\\>");
		text = text.Replace("&gt;", ">");
		text = text.Replace("&lt;", "<");
		text = text.Replace("|", "\\|");
		return text.Replace(Environment.NewLine, "<br>");
	}

	public string Bold(string text)
	{
		return "**" + text + "**";
	}

	public string Code(string text)
	{
		return "`" + text + "`";
	}

	public string Link(string anchorName, string? text)
	{
		return $"[{text}]({anchorName})";
	}

	public string HeadingLink(string anchorName, string? text = null)
	{
		return $"[{text ?? anchorName}](#{anchorName.ToLower().RegexReplace(@"[^a-z\d -]", "").Replace(" ", "-")})";
	}

	#endregion
}
