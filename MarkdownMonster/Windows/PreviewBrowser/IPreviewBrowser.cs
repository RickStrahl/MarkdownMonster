namespace MarkdownMonster.Windows
{
    public interface IPreviewBrowser
    {
        void PreviewMarkdownAsync(MarkdownDocumentEditor editor, bool keepScrollPosition);
        void PreviewMarkdown(MarkdownDocumentEditor editor, bool keepScrollPosition, bool showInBrowser);
    }
}