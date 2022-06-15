namespace JsonEverythingNet.Services.MarkdownGen;

public interface IMarkdownWriter
{
	string FormatName { get; }

	string FullText { get; }
	void Write(string text);
	void WriteLine(string? text = null);

	void WriteH1(string text);
	void WriteH2(string text);
	void WriteH3(string text);
	void WriteH4(string text);

	void WriteTableTitle(params string[] tableHeadings);
	void WriteTableRow(params string?[] texts);
	void WriteLink(string anchorName, string text);
	void WriteHeadingLink(string text);
	void WriteAnchor(string anchorName);
	void WriteHorizontalRule();
	void WriteListItem(string text);
	void WriteCodeBlock(string text);
	string Bold(string text);
	string Link(string anchorName, string? text);
	string HeadingLink(string anchorName, string? text = null);
}