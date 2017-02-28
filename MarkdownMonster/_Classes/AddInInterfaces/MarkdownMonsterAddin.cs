using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using Westwind.Utilities;

namespace MarkdownMonster.AddIns
{
    /// <summary>
    /// Addin Base class that exposes core functionality to the addin
    /// </summary>
    public abstract class MarkdownMonsterAddin : IMarkdownMonsterAddin
    {
        /// <summary>
        /// Optional Id for this addin - use a recognizable Id
        /// </summary>
        public string Id { get; set; } = StringUtils.NewStringId();

        /// <summary>
        /// The display name of the Addin
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The application model which gives you access to Markdown Monster.
        /// Includes access to Configuration and the Main Window
        /// </summary>
        public AppModel Model { get; set; }

        /// <summary>
        /// List of menu items that are used to extend MM
        /// Menu items get attached to the Addin menu and fire
        /// when clicked.        
        /// </summary>
        public List<AddInMenuItem> MenuItems { get; set;  }  = new List<AddInMenuItem>();
        
        
        #region Event Handlers
        

        /// <summary>
        /// Called when the Menu or Toolbar button is clicked
        /// </summary>
        /// <param name="sender"></param>
        public virtual void OnExecute(object sender)
        {
            
        }

        /// <summary>
        /// Called when the configuration Toolbar drop down button is clicked
        /// </summary>
        /// <param name="sender"></param>
        public virtual void OnExecuteConfiguration(object sender)
        {

        }

        /// <summary>
        /// Called to determine whether the menu option should be enabled and execute
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public virtual bool OnCanExecute(object sender)
        {
            return true;   
        }

        /// <summary>
        /// Called when the application starts and after the AddinManager
        /// has initialized.
        /// 
        /// Use this handler to add new menu items to the Addin Toolbar         
        /// </summary>
        public virtual void OnApplicationStart()
        {
          
        }

        /// <summary>
        /// Fired when the main application window has been loaded
        /// Use this event to attach any behaviors/events etc. that
        /// need access to the window
        /// </summary>
        public virtual void OnWindowLoaded()
        {
            
        }

        /// <summary>
        /// Called just before the application is shut down
        /// </summary>
        public virtual void OnApplicationShutdown()
        {
            
        }

        /// <summary>
        /// Called before a document is opened. Return false to 
        /// keep the document from being opened
        /// </summary>
        /// <returns></returns>
        public virtual bool OnBeforeOpenDocument(string filename)
        {
            return true;
        }

        /// <summary>
        /// Called after a new document has been opened. If this is a new
        /// document the filename will be 'untitled'
        /// </summary>
        /// <param name="doc"></param>
        public virtual void  OnAfterOpenDocument(MarkdownDocument doc)
        {            
        }

        /// <summary>
        /// Called before the document is saved. Return false to 
        /// disallow saving the document
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public virtual bool OnBeforeSaveDocument(MarkdownDocument doc)
        {
            return true;
        }


        /// <summary>
        /// An optional command string that is fired into addins         
        /// 
        /// You can override this method to capture commands that are not
        /// already handled by the editor.        
        /// </summary>
        /// <param name="command"></param>
        public virtual void OnNotifyAddin(string command, object parameter)
        {
            
        }


        /// <summary>
        /// Optional editor command handler that can intercept editor commands 
        /// like bold/italic that are fired if not handled previously by
        /// the default handlers.
        /// 
        /// Allows adding custom handlers
        /// </summary>
        /// <param name="command"></param>
        public virtual string OnEditorCommand(string command, string input)
        {
            return null;
        }

        /// <summary>
        /// Called after the document has been saved.
        /// </summary>
        /// <param name="doc"></param>
        public virtual void OnAfterSaveDocument(MarkdownDocument doc)
        {
            
        }

        /// <summary>
        /// Called when an image is to be saved. By default MM saves images to
        /// disk. You can hook this method with your add in to take over the image
        /// save operation. Return true to indicate you handled the         
        /// </summary>        
        /// <param name="image">This parameter holds either a string filename or a Bitmap of the actual image to save</param>
        /// <returns>A Url to link to the image or null to indicate default processing should continue</returns>
        public virtual string OnSaveImage(object image)
        {                                
            return null;
        }
        
        /// <summary>
        /// Called whenever a new document is activated in the editor 
        /// (when tabs change). Note on startup if multiple documents
        /// are open this method is called for each document.
        /// </summary>
        /// <param name="doc"></param>
        public virtual void OnDocumentActivated(MarkdownDocument doc)
        {                                    
        }


        /// <summary>
        /// Called whenever the document is updated and the document's current 
        /// text is updated. Note this may not be always 100% in sync of what's
        /// in the editor as the document is updated only when the user stops
        /// typing for around a second.
        /// </summary>
        public virtual void OnDocumentUpdated()
        {            
        }

        /// <summary>
        /// If this addin wants to provide a custom Markdown Parser this method can 
        /// be overriden to do it.
        /// </summary>
        /// <returns>IMarkdownParser instance or null. Passed the instance is used for parsing</returns>
        public virtual IMarkdownParser GetMarkdownParser()
        {
            return null;           
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Retrieve an instance of the MarkdownEditor control from the
        /// active window. This instance wraps the editor and provides
        /// a number of methods for getting access to the editor document
        /// </summary>
        /// <returns></returns>
        protected MarkdownDocumentEditor GetMarkdownEditor()
        {
            return Model.Window.GetActiveMarkdownEditor();
        }

        /// <summary>
        /// Returns the Markdown document instance which has access
        /// to the actual document that ends up being saved. This
        /// includes the content and the IsDirty flag. Note that
        /// content is updated only after save operations. If you need
        /// to update live content it's best to inject directly into
        /// the edtor using the GetSelectedText() and SetSelectedText().
        /// </summary>
        /// <returns></returns>
        protected internal MarkdownDocument GetMarkdownDocument()
        {
            var editor =  Model.Window.GetActiveMarkdownEditor();
            return editor?.MarkdownDocument;
        }

        /// <summary>
        /// Returns the active live markdown text from the editor
        /// </summary>
        /// <returns></returns>
        protected string GetMarkdown()
        {
            var editor = Model.Window.GetActiveMarkdownEditor();
            return editor?.GetMarkdown();
        }


        /// <summary>
        /// Sets all the text in the markdown editor
        /// </summary>
        /// <param name="markdownText"></param>
        protected void SetMarkdown(string markdownText)
        {
            var editor = this.Model.Window.GetActiveMarkdownEditor();
            editor?.SetMarkdown(markdownText);
        }


        /// <summary>
        /// Gets the active selection from the editor
        /// </summary>
        /// <returns></returns>
        protected string GetSelection()
        {
            var editor = this.Model.Window.GetActiveMarkdownEditor();
            return editor?.AceEditor.getselection(false);
        }
        
        /// <summary>
        /// Sets the active selection from the editor
        /// </summary>
        /// <param name="text"></param>
        protected void SetSelection(string text)
        {
            var editor = this.Model.Window.GetActiveMarkdownEditor();
            if (editor == null)
                return;

            if (!string.IsNullOrEmpty(text))
                editor.AceEditor.setselection(text);

            editor.WebBrowser.Focus();
            editor.AceEditor.setfocus(true);

            editor.MarkdownDocument.CurrentText = editor.GetMarkdown();
            Model.Window.PreviewMarkdown(editor, true);            
        }


        /// <summary>
        /// Brings the editor to focus
        /// </summary>
        protected void SetEditorFocus()
        {
            Model.ActiveEditor.SetEditorFocus();
        }


        /// <summary>
        /// Executes a predefined edit command (bold,italic,href etc.) 
        /// against the editor.
        /// </summary>
        /// <param name="action">Name of the Editor action to perform</param>
        protected void ExecuteEditCommand(string action)
        {
            var editor = this.Model.Window.GetActiveMarkdownEditor();
            editor?.ProcessEditorUpdateCommand(action);
        }


        /// <summary>
        /// Opens a tab with a given filename and selects it
        /// </summary>
        /// <param name="filename">File to open</param>
        /// <returns>The TabItem instance representing the opened tab</returns>
        protected TabItem OpenTab(string filename)
        {
            return Model.Window.OpenTab(filename);                        
        }


        /// <summary>
        /// Closes a specific tab that you pass. You can look at
        /// the tab collection via Model.Window.TabControl.
        /// </summary>
        /// <param name="tab"></param>
        protected void CloseTab(TabItem tab)
        {
            Model.Window.CloseTab(tab);
        }

        /// <summary>
        /// Closes the tab that contains the file specified by 
        /// filename
        /// </summary>
        /// <param name="filename"></param>
        protected void CloseTab(string filename)
        {
            Model.Window.CloseTab(filename);
        }

        /// <summary>
        /// Refreshes the Preview WebBrowser
        /// </summary>
        protected void UpdatePreview()
        {            
            Model.Window.PreviewMarkdownAsync();
        }


        /// <summary>
        /// Shows a Status Message on the Status bar
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="timeoutMs">optional timeout in milliseconds</param>
        protected void ShowStatus(string message, int timeoutMs = 0)
        {            
            Model.Window.ShowStatus(message, timeoutMs);            
        }


        /// <summary>
        /// Lets you modify the status icon and color on the status bar.
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="color"></param>
        /// <param name="spin"></param>
        protected void SetStatusIcon(FontAwesome.WPF.FontAwesomeIcon icon, Color color,bool spin = false)
        {
            Model.Window.SetStatusIcon(icon, color,spin);
            
            
        }
        #endregion

        public override string ToString()
        {
            return Id ?? "No Addin Id specified";
        }

        
    }   
}
