using System;

namespace MarkdownMonster.Windows.PreviewBrowser
{
    public interface IPreviewBrowser :IDisposable
    {
        
        void PreviewMarkdown(MarkdownDocumentEditor editor=null, bool keepScrollPosition = false, bool showInBrowser=false,string renderedHtml = null, int editorLineNumber = -1);

        void PreviewMarkdownAsync(MarkdownDocumentEditor editor=null, bool keepScrollPosition = false, string renderedHtml = null, int editorLineNumber = -1);

        /// <summary>
        /// Scrolls to editor line and highlights the active line if it can be matched
        /// in the preview.
        /// </summary>
        /// <param name="editorLineNumber">Line to go to</param>
        /// <param name="headerId">Optionally go to a header id</param>
        /// <param name="noScrollTimeout">Fire scroll events immediately</param>
        /// <param name="noScrollTopAdjustment">Don't scroll just highlight - good for cursor navigations</param>
        void ScrollToEditorLine(int editorLineNumber = -1, bool updateCodeBlocks = false, bool noScrollContentTimeout = false, bool noScrollTopAdjustment = false);

        
        /// <summary>
        /// Scrolls to editor line and highlights the active line if it can be matched
        /// in the preview.
        /// </summary>
        /// <param name="editorLineNumber">Line to go to</param>
        /// <param name="headerId">Optionally go to a header id</param>
        /// <param name="noScrollTimeout">Fire scroll events immediately</param>
        /// <param name="noScrollTopAdjustment">Don't scroll just highlight - good for cursor navigations</param>        void ScrollToEditorLineAsync(int editorLineNumber = -1, bool updateCodeBlocks = false, bool noScrollContentTimeout = false, bool noScrollTopAdjustment = false);
        void ScrollToEditorLineAsync(int editorLineNumber = -1, bool updateCodeBlocks = false, bool noScrollContentTimeout = false, bool noScrollTopAdjustment = false);

        void Navigate(string url);

        void Refresh(bool noCache);

        bool IsVisible { get; }


        void ExecuteCommand(string command, params dynamic[] args);

        void ShowDeveloperTools();
    }
}
