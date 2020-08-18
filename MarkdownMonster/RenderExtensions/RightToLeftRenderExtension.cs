using System;
using System.IO;
using System.Text;
using Markdig.Extensions.Globalization;
using Westwind.Utilities;

namespace MarkdownMonster.RenderExtensions
{
    /// <summary>
    /// This is the base Markdown Monster Render extension to handle miscellaneous cleanups
    /// </summary>
    public class RightToLeftRenderExtension : IRenderExtension
    {

        /// <summary>
        /// Handle Right To Left ACE Editor markdown removal so rendering looks correct
        /// </summary>
        /// <param name="args"></param>
        public void BeforeMarkdownRendered(ModifyMarkdownArguments args)
        {
            // Right to Left Rendering fix up from ACE Editor
            if (mmApp.Configuration.Editor.EnableRightToLeft )
            {
                if (!string.IsNullOrEmpty(args.Markdown)) {
                    // HACK: Strip ACE embed RTL/LTR Transition character
                    var bytes = args.MarkdownDocument.Encoding.GetBytes(args.Markdown);
                    var bytesToRemove = new byte[] {0xe2, 0x80, 0xAb};
                    if (DataUtils.IndexOfByteArray(bytes, bytesToRemove) > -1)
                    {
                        var newbytes = DataUtils.RemoveBytes(bytes, bytesToRemove);
                        args.Markdown = args.MarkdownDocument.Encoding.GetString(newbytes);
                    }
                }
            }
        }


        /// <summary>
        /// Nothing to do
        /// </summary>
        /// <param name="args"></param>
        public void AfterMarkdownRendered(ModifyHtmlAndHeadersArguments args)
        {
           
        }


        /// <summary>
        /// Nothing to do.
        /// </summary>
        /// <param name="args"></param>
        public void AfterDocumentRendered(ModifyHtmlArguments args)
        {
            

        }

    }
}
