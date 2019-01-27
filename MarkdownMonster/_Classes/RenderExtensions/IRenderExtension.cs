using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownMonster.RenderExtensions
{
    /// <summary>
    /// Interface implemented for RenderExtensions that modify
    /// </summary>
    public interface IRenderExtension
    {
        /// <summary>
        /// Renders HTML Head content if required
        /// </summary>
        /// <param name="html"></param>
        /// <param name="markdown"></param>
        /// <returns></returns>
        string RenderHeader(string html, string markdown, MarkdownDocument document);


        /// <summary>
        /// Use to insert content into the HTML output by replacing
        /// the content of the passed in HTML ref parameter.
        /// </summary>
        /// <param name="html">Passed by ref to signify that you can mutate the HTML to be rendered</param>
        /// <param name="markdown">original markdown conent passed in for reference</param>
        void InsertContent(ref string html, string markdown, MarkdownDocument document);

        
        /// <summary>
        /// Return true or false depending on whether this extension should process.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="markdown"></param>
        /// <returns></returns>
        bool ShouldProcess(string html, string markdown, MarkdownDocument document);


        /// <summary>
        /// Method that is fired on the inbound pass before the document is rendered and that
        /// allows you to modify the markdown before it is sent out for rendering.
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="document"></param>
        void BeforeRender(ref string markdown, MarkdownDocument document);

        /// <summary>
        /// Determines whether BeforeRender should be processed
        /// </summary>
        /// <param name="markdown"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        bool ShouldProcessBeforeRender(string markdown, MarkdownDocument document);
    }
}
