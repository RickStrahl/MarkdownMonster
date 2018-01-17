namespace MarkdownMonster.Windows
{
    public interface IPreviewBrowser
    {
        void PreviewMarkdownAsync(MarkdownDocumentEditor editor, bool keepScrollPosition, string renderedHtml);
        void PreviewMarkdown(MarkdownDocumentEditor editor, bool keepScrollPosition, bool showInBrowser,string renderedHtml);
    }
}
