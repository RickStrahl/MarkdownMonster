#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 * 
 * Created: 04/28/2016
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using MarkdownMonster.Windows;
using MarkdownMonster.Windows.PreviewBrowser;
using Westwind.Utilities;

namespace MarkdownMonster.AddIns
{
    /// <summary>
    /// Addin Base class that exposes core functionality to the addin.
    ///     
    /// </summary>
    public abstract class MarkdownMonsterAddin : IMarkdownMonsterAddin
    {
        #region Addin Configuration
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
        /// Determines whether this addin is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// List of menu items that are used to extend MM
        /// Menu items get attached to the Addin menu and fire
        /// when clicked.        
        /// </summary>
        public List<AddInMenuItem> MenuItems { get; set;  }  = new List<AddInMenuItem>();

        #endregion

        #region Access Properties

        /// <summary>
        /// Returns an instance of the Active Editor instance. The editor contains
        /// editor behavior of the browser control as well as all interactions with
        /// the editor's event model and text selection interfaces.
        ///         
        /// Contains an `AceEditor` property that references the underlying 
        /// JavaScript editor wrapper instance.
        /// </summary>
        public MarkdownDocumentEditor ActiveEditor => Model.ActiveEditor;


        /// <summary>
        /// Returns the active Markdown document that's being edited. The document
        /// holds the actual markdown text and knows how to load, save and render
        /// the markdown contained within it.
        /// </summary>
        public MarkdownDocument ActiveDocument => Model.ActiveDocument;


        #endregion


        
        #region Event Handlers

        /// <summary>
        /// Allows addins to intercept the html used for the preview, to
        /// examine or further manipulate it, e.g. insert a style
        /// block in the head.                        
        /// </summary>
        /// <remarks>
        /// If multiple addins are hooked in to modify the preview html 
        /// you may get unpredictable results.
        /// </remarks>
        public virtual string OnModifyPreviewHtml(string renderedHtml, string markdownHtml)
        {
            return renderedHtml;
        }

        /// <summary>
        /// Called when the Menu or Toolbar button is clicked
        /// </summary>
        /// <param name="sender">Menu item clicked</param>
        public virtual void OnExecute(object sender)
        {            
        }

        /// <summary>
        /// Called when the configuration Toolbar drop down button is clicked
        /// </summary>
        /// <param name="sender">Menu Item clicked</param>
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
        /// Use this handler to add new menu items to the Addin Toolbar.                       
        /// </summary>
        /// <remarks>
        /// Fires very early in the load cycle and therefore has no access
        /// to the App Model or UI. If you require access to Model or UI
        /// override `OnWindowLoaded()` instead.
        /// </remarks>
        public virtual void OnApplicationStart()
        {
          
        }


        /// <summary>
        /// Fired when the application has Initialized the Model. This happens
        /// after OnApplicationStart() but before OnWindowLoaded() and allows
        /// you to access the Model before initial data binding of the Window 
        /// occurs.
        /// </summary>
        /// <param name="model">Instance of the Markdown Monster Application Model</param>
        public virtual void OnModelLoaded(AppModel model)
        {
            Model = model;
        }

        /// <summary>
        /// Fired when the main application window has been loaded and the
        /// main Markdown Monster Application Model is available. 
        /// Access the Window with mmApp.Model.Window.
        ///
        /// Use this method to ensure that Model and UI are available, 
        /// often in combination with `OnApplicationStart()` and `GetMarkdownParser()`
        /// which fire before the Model or UI are active.
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
        /// Called after the document has been saved.
        /// </summary>
        /// <param name="doc"></param>
        public virtual void OnAfterSaveDocument(MarkdownDocument doc)
        {
            
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
        [Obsolete("Please use the GetMarkdownDownParser(bool usePragmaLines, bool force) overload.")]
        public virtual IMarkdownParser GetMarkdownParser()
        {
            return null;           
        }

        /// <summary>
        /// If this addin wants to provide a custom Markdown Parser this method can 
        /// be overriden to do it.
        /// </summary>
        /// <param name="usePragmaLines">If true, pragma line ids should be added into the document
        /// to support preview synchronization</param>
        /// <param name="force">Forces the parser to be reloaded - otherwise previously loaded instance can be used</param>
        /// <returns>IMarkdownParser instance or null. Passed the instance is used for parsing</returns>
        public virtual IMarkdownParser GetMarkdownParser(bool usePragmaLines, bool force)
        {
            // Existing parsers use the older method, so default to calling that.
#pragma warning disable CS0618 // Type or member is obsolete
            return GetMarkdownParser();
#pragma warning restore CS0618 // Type or member is obsolete

        }

        /// <summary>
        /// Allows returning a WPF control that implements IPreviewBrowser and 
        /// that handles previewing the output from documents.
        /// 
        /// This control should return an IPreviewBrowser interface implemented
        /// on a WPF UIControl (UserControl most likely).
        /// </summary>
        /// <returns></returns>
        public virtual IPreviewBrowser GetPreviewBrowserUserControl()
        {
            return null;
        }

        /// <summary>
        /// Called after the addin is initially installed. Use this
        /// method to install additional resources or add additional
        /// one time configuration.
        /// </summary>
        public virtual void OnInstall()
        {            
        }


        /// <summary>
        /// Called after the addin has been uninstalled. Allows
        /// for additional cleanup.
        /// </summary>
        public virtual void OnUninstall()
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
        public MarkdownDocumentEditor GetMarkdownEditor()
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
        public MarkdownDocument GetMarkdownDocument()
        {
            var editor =  Model.Window.GetActiveMarkdownEditor();
            return editor?.MarkdownDocument;
        }

        /// <summary>
        /// Returns the active live markdown text from the editor
        /// </summary>
        /// <returns></returns>
        public string GetMarkdown()
        {
            var editor = Model.Window.GetActiveMarkdownEditor();
            return editor?.GetMarkdown();
        }


        /// <summary>
        /// Sets all the text in the markdown editor
        /// </summary>
        /// <param name="markdownText"></param>
        public void SetMarkdown(string markdownText)
        {
            var editor = Model.Window.GetActiveMarkdownEditor();
            editor?.SetMarkdown(markdownText);
        }


        /// <summary>
        /// Gets the active selection from the editor
        /// </summary>
        /// <returns></returns>
        public string GetSelection()
        {
            return Model.ActiveEditor?.AceEditor.getselection(false) ?? string.Empty;
        }
        
        /// <summary>
        /// Sets the active selection from the editor
        /// </summary>
        /// <param name="text"></param>
        public void SetSelection(string text)
        {
            var editor = Model.Window.GetActiveMarkdownEditor();
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
        public void SetEditorFocus()
        {
            Model.Window.Activate();
            Model.ActiveEditor?.SetEditorFocus();
        }


        /// <summary>
        /// Refreshes the Html Preview Window if active
        /// </summary>
        /// <param name="keepScrollPosition"></param>
        public void RefreshPreview(bool keepScrollPosition=true)
        {
            Model.Window.PreviewMarkdownAsync(keepScrollPosition: keepScrollPosition);
        }

        /// <summary>
        /// Executes a predefined edit command (bold,italic,href etc.) 
        /// against the editor.
        /// </summary>
        /// <param name="action">Name of the Editor action to perform</param>
        public void ExecuteEditCommand(string action)
        {
            var editor = Model.Window.GetActiveMarkdownEditor();
            editor?.ProcessEditorUpdateCommand(action);
        }


        /// <summary>
        /// Opens a tab with a given filename and selects it
        /// </summary>
        /// <param name="filename">File to open</param>
        /// <returns>The TabItem instance representing the opened tab</returns>
        public TabItem OpenTab(string filename)
        {
            return Model.Window.OpenTab(filename, rebindTabHeaders: true);
        }


        /// <summary>
        /// Closes a specific tab that you pass. You can look at
        /// the tab collection via Model.Window.TabControl.
        /// </summary>
        /// <param name="tab"></param>
        public void CloseTab(TabItem tab)
        {
            Model.Window.CloseTab(tab);
        }

        /// <summary>
        /// Closes the tab that contains the file specified by 
        /// filename
        /// </summary>
        /// <param name="filename"></param>
        public void CloseTab(string filename)
        {
            Model.Window.CloseTab(filename);
        }


        /// <summary>
        /// Shows a Status Message on the Status bar
        /// </summary>
        /// <param name="message">Message to display</param>
        /// <param name="timeoutMs">optional timeout in milliseconds</param>
        public void ShowStatus(string message, int timeoutMs = 0)
        {            
            Model.Window.ShowStatus(message, timeoutMs);            
        }


        /// <summary>
        /// Lets you modify the status icon and color on the status bar.
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="color"></param>
        /// <param name="spin"></param>
        public void SetStatusIcon(FontAwesome.WPF.FontAwesomeIcon icon, Color color,bool spin = false)
        {
            Model.Window.SetStatusIcon(icon, color,spin);
            
            
        }
        #endregion

        #region UI Shell Operations

        /// <summary>
        /// Allows insertion of a menu item 
        /// </summary>
        /// <param name="mitem">The menu item to insert</param>
        /// <param name="menuItemNameForInsertionAfter">Name of the main menuitem element to insert before or after - find in MainWindow.xaml or with Debug Tools</param>
        /// <param name="menuItemTextForInsertionAfter">Text of the menuitem element to insert bfore or after (use if there's is no explicit Name for the item)</param>
        /// <param name="mode">0 - insert after, 1 - insert before, 2 - replace</param>
        public bool AddMenuItem(MenuItem mitem, string menuItemNameForInsertionAfter = null, string menuItemTextForInsertionAfter = null, int mode = 0)
        {
            // find the menu item to in
            var menuItem = GetChildMenuItem(Model.Window.MainMenu, menuItemNameForInsertionAfter, menuItemTextForInsertionAfter);
            if (menuItem == null)
                return false;

            ItemsControl parent = menuItem.Parent as ItemsControl;
            if (parent == null)
                return false;

            int idx;
            if (mode == 1)
            {
                idx = parent.Items.IndexOf(menuItem);
            }
            else if (mode == 2)
            {
                idx = parent.Items.IndexOf(menuItem);
                parent.Items[idx] = mitem;                
                return true;
            }
            else
            {
                idx = parent.Items.IndexOf(menuItem);
                idx++;
            
            }
            parent.Items.Insert(idx, mitem);

            return true;
        }

        /// <summary>
        /// Use this to find a menu item either by control name or by 
        /// caption text.
        /// 
        /// Pass either menuItemName OR menuItemText parameter. If both are
        /// passed menuItemName takes precendence.
        /// </summary>
        /// <param name="mitem"></param>
        /// <param name="menuItemName"></param>
        /// <param name="menuItemText"></param>
        /// <returns></returns>
        public MenuItem GetChildMenuItem(ItemsControl mitem, string menuItemName = null, string menuItemText = null)        
        {
            foreach (var control in mitem.Items)
            {                
                var menuItem = control as MenuItem;
                if (menuItem == null)
                    continue;


                if (!string.IsNullOrEmpty(menuItemName) && menuItemName == menuItem.Name)
                    return menuItem;

                if (!string.IsNullOrEmpty(menuItemText) && menuItemName == menuItem.Header?.ToString())
                    return menuItem;

                if (menuItem.Items != null)
                {
                    menuItem = GetChildMenuItem(menuItem, menuItemName, menuItemText);
                    if (menuItem != null)
                        return menuItem;
                }
            }

            return null;
        }
        #endregion

        /// <summary>
        /// Customized to display the Addin Id or Name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Id ?? Name ?? "no name";
        }
    }    
}
