namespace MarkdownMonster.Windows.PreviewBrowser
{
    public interface IPreviewBrowser
    {
        void PreviewMarkdownAsync(MarkdownDocumentEditor editor=null, bool keepScrollPosition = false, string renderedHtml = null);

        void PreviewMarkdown(MarkdownDocumentEditor editor=null, bool keepScrollPosition = false, bool showInBrowser=false,string renderedHtml = null);

        void Navigate(string url);

        bool IsVisible { get; }


        void ExecuteCommand(string command, params dynamic[] args);
    }
}
