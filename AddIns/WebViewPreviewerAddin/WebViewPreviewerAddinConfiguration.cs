using MarkdownMonster.AddIns;

namespace WebViewPreviewerAddin
{
    public class WebViewPreviewerAddinConfiguration : BaseAddinConfiguration<WebViewPreviewerAddinConfiguration>
    {
        /// <summary>
        /// Determines whether the addin is enabled
        /// </summary>
        public bool IsActive {get; set; } = false;


        public WebViewPreviewerAddinConfiguration()
        {
            // uses this file for storing settings in `%appdata%\Markdown Monster`
            // to persist settings call `WebViewPreviewerAddinConfiguration.Current.Write()`
            // at any time or when the addin is shut down
            ConfigurationFilename = "WebViewPreviewerAddin.json";
        }

        // Add properties for any configuration setting you want to persist and reload
        // you can access this object as 
        //     WebViewPreviewerAddinConfiguration.Current.PropertyName
    }
}