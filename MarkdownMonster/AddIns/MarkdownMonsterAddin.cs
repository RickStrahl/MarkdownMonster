using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownMonster.AddIns
{
    public abstract class MarkdownMonsterAddin : IMarkdownMonsterAddin
    {
        public AppModel Model { get; set; }

        /// <summary>
        /// List of menu items that are used to extend MM
        /// Menu items get attached to the Addin menu and fire
        /// when clicked.        
        /// </summary>
        public List<AddInMenuItem> MenuItems { get; set;  }  = new List<AddInMenuItem>();



        #region Event Handlers

        /// <summary>
        /// Called when the application starts and after the AddinManager
        /// has initialized.
        /// 
        /// Use this handler to add new menu items to the Addin Toolbar         
        /// </summary>
        public virtual void OnApplicationStart()
        {
          
        }

        public virtual void OnApplicationShutdown()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool OnBeforeOpenFile()
        {
            return true;
        }

        public virtual void  OnAfterOpenFile()
        {            
        }

        public virtual bool OnBeforeSaveFile()
        {
            return true;
        }

        public virtual void OnAfterSaveFile()
        {
            
        }

        public virtual void OnDocumentActivated()
        {
                        
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Retrieve an instance of the MarkdownEditor control from the
        /// active window. This instance wraps the editor and provides
        /// a number of methods for getting access to the editor document
        /// </summary>
        /// <returns></returns>
        protected internal MarkdownDocumentEditor GetMarkdownEditor()
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
        protected internal string GetMarkdown()
        {
            var editor = this.Model.Window.GetActiveMarkdownEditor();
            return editor?.GetMarkdown();
        }


        /// <summary>
        /// Sets all the text in the markdown editor
        /// </summary>
        /// <param name="markdownText"></param>
        protected internal void SetMarkdown(string markdownText)
        {
            var editor = this.Model.Window.GetActiveMarkdownEditor();
            editor?.SetMarkdown(markdownText);
        }


        /// <summary>
        /// Gets the active selection from the editor
        /// </summary>
        /// <returns></returns>
        protected internal string GetSelection()
        {
            var editor = this.Model.Window.GetActiveMarkdownEditor();
            return editor?.AceEditor.getselection(false);
        }


        /// <summary>
        /// Sets the active selection from the editor
        /// </summary>
        /// <param name="text"></param>
        protected internal void SetSelection(string text)
        {
            var editor = this.Model.Window.GetActiveMarkdownEditor();
            if (editor == null)
                return;

            editor.AceEditor.setselection(text);
            editor.WebBrowser.Focus();
            editor.AceEditor.setfocus(true);

            editor.MarkdownDocument.CurrentText = editor.GetMarkdown();
            Model.Window.PreviewMarkdown(editor, true);            
        }

        /// <summary>
        /// Executes a predefined edit command (bold,italic,href etc.) 
        /// against the editor.
        /// </summary>
        /// <param name="cmd"></param>
        protected internal void ExecuteEditCommand(string action)
        {
            var editor = this.Model.Window.GetActiveMarkdownEditor();
            editor?.ProcessEditorUpdateCommand(action);
        }
        #endregion
    }   
}
