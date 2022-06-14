namespace JsonEverythingNet.Services.MarkdownGen.MarkdownWriters.Interfaces
{
    public interface IMarkdownWriter
    {
        string FormatName { get; }
        void Write(string text);
        void WriteLine(string text);

        void WriteH1(string text);
        void WriteH2(string text);
        void WriteH3(string text);


        void WriteTableTitle(params string[] tableHeadings);
        void WriteTableRow(params string[] texts);
        void WriteLink(string anchorName, string text);
        void WriteHeadingLink(string text);
        void WriteAnchor(string anchorName);
        void WriteHorizontalRule();
        string Bold(string text);
        string Link(string anchorName, string text);
        string HeadingLink(string anchorName, string text = null);

        string FullText { get; }
    }
}
