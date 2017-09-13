using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

[assembly: TagPrefix("Markdown", "ww")]


    
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:Markdown runat=server></{0}:Markdown>")]

    /// <summary>
    /// Summary description for MarkdownControl
    /// </summary>
    public class Markdown : Label
    {
        public Markdown()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        protected override void RenderContents(HtmlTextWriter writer)
        {
            //base.RenderContents(writer);
            writer.Write("---Markdown---<hr/>");
            writer.Write(Text);
            writer.Write("<hr />---Markdown---");
        }
    }
