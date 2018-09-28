using System;

namespace MarkdownMonster.Windows.PreviewBrowser
{
    public interface IPreviewBrowser :IDisposable
    {
        void PreviewMarkdownAsync(MarkdownDocumentEditor editor=null, bool keepScrollPosition = false, string renderedHtml = null, int editorLineNumber = -1);

        void PreviewMarkdown(MarkdownDocumentEditor editor=null, bool keepScrollPosition = false, bool showInBrowser=false,string renderedHtml = null, int editorLineNumber = -1);

        void Navigate(string url);

        void Refresh(bool noCache);

        bool IsVisible { get; }


        void ExecuteCommand(string command, params dynamic[] args);        
    }
}
