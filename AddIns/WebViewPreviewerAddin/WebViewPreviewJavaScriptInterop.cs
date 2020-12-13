using System;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster;
using Microsoft.Web.WebView2.Wpf;
using Westwind.Utilities;

namespace WebViewPreviewerAddin
{

    /// <summary>
    /// This class is used to call into the JavaScript document and perform
    /// operations there.
    ///
    /// Note there's no inheritance/Composition as this interface requires
    /// Async operation, while the COM interface for WebBrowser control
    /// requires sync operation.
    /// </summary>
    public class WebViewPreviewJavaScriptInterop
    {
        private WebViewPreviewDotnetInterop _webViewPreviewDotnetInterop;
        private WebView2 WebBrowser;


        public WebViewPreviewJavaScriptInterop(WebViewPreviewDotnetInterop interop)
        {
            _webViewPreviewDotnetInterop = interop;
            WebBrowser = interop.WebBrowser as WebView2;
        }


        /// <summary>
        /// Initialize the document
        /// </summary>
        public async Task InitializeInterop()
        {
            await CallMethod("initializeinterop");
        }


        /// <summary>
        /// Update the document with an HTML string. Optional line number
        /// on where to scroll the document to.
        /// </summary>
        /// <param name="html"></param>
        /// <param name="lineNo"></param>
        public async Task UpdateDocumentContent(string html, int lineNo)
        {
            if (_webViewPreviewDotnetInterop.WebBrowser == null)
                return;
            await CallMethod("updateDocumentContent", html, lineNo);
        }


        /// <summary>
        /// Scroll to a specific line in the document
        /// </summary>
        /// <param name="editorLineNumber"></param>
        /// <param name="headerId"></param>
        /// <param name="updateCodeBlocks"></param>
        /// <param name="noScrollTimeout"></param>
        /// <param name="noScrollTopAdjustment"></param>
        /// <returns></returns>
        public async Task ScrollToPragmaLine(int editorLineNumber = -1,
            string headerId = null,
            bool updateCodeBlocks = true,
            bool noScrollTimeout = false, bool noScrollTopAdjustment = false)
        {
            await CallMethod("scrollToPragmaLine", editorLineNumber, headerId, noScrollTimeout, noScrollTopAdjustment);
        }

        #region Async Invocation Utilities
        /// <summary>
        /// Calls a method with simple or no parameters: string, boolean, numbers
        /// </summary>
        /// <param name="method">Method to call</param>
        /// <param name="parameters">Parameters to path or none</param>
        /// <returns>object result as specified by TResult type</returns>
        public async Task<TResult> CallMethod<TResult>(string method, params object[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("textEditor." + method + "(");

            if (parameters != null)
            {
                for (var index = 0; index < parameters.Length; index++)
                {
                    object parm = parameters[index];
                    var jsonParm = JsonSerializationUtils.Serialize(parm);
                    sb.Append(jsonParm);
                    if (index < parameters.Length - 1)
                        sb.Append(",");
                }
            }
            sb.Append(")");

            var cmd = sb.ToString();
            string result = await WebBrowser.CoreWebView2.ExecuteScriptAsync(cmd);
            
            Type resultType = typeof(TResult);
            return (TResult) JsonSerializationUtils.Deserialize(result, resultType, true);
        }

        /// <summary>
        /// Calls a method with simple parameters: String, number, boolean
        /// This version returns no results.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task CallMethod(string method, params object[] parameters)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(method + "(");

            if (parameters != null)
            {
                for (var index = 0; index < parameters.Length; index++)
                {
                    object parm = parameters[index];
                    var jsonParm = JsonSerializationUtils.Serialize(parm);
                    sb.Append(jsonParm);
                    if (index < parameters.Length - 1)
                        sb.Append(",");
                }
            }
            sb.Append(")");

            await WebBrowser.CoreWebView2.ExecuteScriptAsync(sb.ToString());
        }

        
        /// <summary>
        /// Calls a method on the TextEditor in JavaScript a single JSON encoded
        /// value or object. The receiving function should expect a JSON object and parse it.
        ///
        /// This version returns no result value.
        /// </summary>
        public async Task CallMethodWithJson(string method, object parameter = null)
        {
            string cmd =  method;

            if (parameter != null)
            {
                var jsonParm = JsonSerializationUtils.Serialize(parameter);
                cmd += "(" + jsonParm + ")";
            }

            await WebBrowser.CoreWebView2.ExecuteScriptAsync(cmd);
        }

        /// <summary>
        /// Calls a method on the TextEditor in JavaScript a single JSON encoded
        /// value or object. The receiving function should expect a JSON object and parse it.
        /// </summary>
        /// <param name="method"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public async Task<object> CallMethodWithJson<TResult>(string method, object parameter = null)
        {
            string cmd = method;

            if (parameter != null)
            {
                var jsonParm = JsonSerializationUtils.Serialize(parameter);
                cmd += "(" + jsonParm + ")";
            }

            string result = await WebBrowser.CoreWebView2.ExecuteScriptAsync(cmd);
            return JsonSerializationUtils.Deserialize(result, typeof(TResult), true);
        }
        #endregion

    }
}
