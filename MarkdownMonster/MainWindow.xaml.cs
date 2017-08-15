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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using Dragablz;
using FontAwesome.WPF;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using MarkdownMonster.AddIns;
using MarkdownMonster.Windows;
using Westwind.Utilities;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using Clipboard = System.Windows.Clipboard;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Cursors = System.Windows.Input.Cursors;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using TextDataFormat = System.Windows.TextDataFormat;
using WebBrowser = System.Windows.Controls.WebBrowser;

namespace MarkdownMonster
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : MetroWindow
	{
		public AppModel Model { get; set; }

		private NamedPipeManager PipeManager { get; set; }

		public IntPtr Hwnd
		{
			get
			{
				if (_hwnd == IntPtr.Zero)
					_hwnd = new WindowInteropHelper(this).EnsureHandle();

				return _hwnd;
			}
		}

		private IntPtr _hwnd = IntPtr.Zero;

		private DateTime invoked = DateTime.MinValue;

		/// <summary>
		/// Handles WebBrowser configuration: DPI Awareness mainly!
		/// </summary>
		private WebBrowserHostUIHandler wbHandler;


		public MainWindow()
		{
			InitializeComponent();

			Model = new AppModel(this);
			DataContext = Model;

			InitializePreviewBrowser();

			TabControl.ClosingItemCallback = TabControlDragablz_TabItemClosing;

			Loaded += OnLoaded;
			Drop += MainWindow_Drop;
			AllowDrop = true;			
			Activated += OnActivated;

			// Singleton App startup - server code that listens for other instances
			if (mmApp.Configuration.UseSingleWindow)
			{
				// Listen for other instances launching and pick up
				// forwarded command line arguments
				PipeManager = new NamedPipeManager("MarkdownMonster");
				PipeManager.StartServer();
				PipeManager.ReceiveString += HandleNamedPipe_OpenRequest;
			}

			// Override some of the theme defaults (dark header specifically)
			mmApp.SetThemeWindowOverride(this);

			// Forces WebBrowser to be DPI aware and not display script errors
			wbHandler = new WebBrowserHostUIHandler(PreviewBrowser);
		}

		#region Opening and Closing

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			RestoreSettings();

            RecentDocumentsContextList();
			ButtonRecentFiles.ContextMenu = Resources["ContextMenuRecentFiles"] as ContextMenu;

            OpenFilesFromCommandLine();

		    if (mmApp.Configuration.ApplicationUpdates.FirstRun)
			{
				if (TabControl.Items.Count == 0)
				{
				    try
				    {
				        string tempFile = Path.Combine(Path.GetTempPath(), "SampleMarkdown.md");
				        File.Copy(Path.Combine(Environment.CurrentDirectory, "SampleMarkdown.md"), tempFile, true);
				        OpenTab(tempFile);
				    }
				    catch (Exception ex)
				    {
				        mmApp.Log("Handled: Unable to copy file to temp folder.", ex);
				    }
				}
				mmApp.Configuration.ApplicationUpdates.FirstRun = false;
			}

			BindTabHeaders();

			if (mmApp.Configuration.IsPreviewVisible)
			{
				ButtonHtmlPreview.IsChecked = true;
				ToolButtonPreview.IsChecked = true;
				//Model.PreviewBrowserCommand.Execute(ButtonHtmlPreview);
			}

			Model.IsPresentationMode = mmApp.Configuration.OpenInPresentationMode;
			if (Model.IsPresentationMode)
			{
				Model.PresentationModeCommand.Execute(ToolButtonPresentationMode);
				Model.IsPreviewBrowserVisible = true;
			}

			var left = Left;
			Left = 300000;

			// force controls to realign - required because of WebBrowser control weirdness            
			Dispatcher.InvokeAsync(() =>
			{
				//TabControl.InvalidateVisual();
				Left = left;				

				mmApp.SetWorkingSet(10000000, 5000000);
			}, DispatcherPriority.Background);


			new TaskFactory().StartNew(() =>
			{
				Dispatcher.Invoke(() =>
				{
					FixMonitorPosition();

					AddinManager.Current.InitializeAddinsUi(this);
					AddinManager.Current.RaiseOnWindowLoaded();
				}, DispatcherPriority.ApplicationIdle);
			});
		}


        /// <summary>
        /// Opens files from the command line or from an array of strings
        /// </summary>
        /// <param name="args">Array of file names. If null Command Line Args are used.</param>
	    private void OpenFilesFromCommandLine(string[] args = null)
	    {
	        if (args == null)
	        {
                // read fixed up command line args
	            args = App.commandArgs; 

	            if (args == null || args.Length == 0) // no args, only command line
	                return;
	        }
	        
	        foreach (var fileArgs in args)
	        {
	            var file = fileArgs;
                if (string.IsNullOrEmpty(file))
                    continue;

	            file = file.TrimEnd('\\');
	            
	            try
	            {
                    // FAIL: This fails at runtime not in debugger when value is .\ trimmed to . VERY WEIRD
	                file = Path.GetFullPath(file);
	            }
	            catch
	            {
	                mmApp.Log("Fullpath CommandLine failed: " + file);                    
	            }

	            if (File.Exists(file))
	                OpenTab(mdFile: file, batchOpen: true);
	            else if (Directory.Exists(file))
	                ShowFolderBrowser(false, file);
	            else
	            {
	                file = Path.Combine(App.initialStartDirectory, file);
	                file = Path.GetFullPath(file);
	                if (File.Exists(file))
	                    OpenTab(mdFile: file, batchOpen: true);
	                else if (Directory.Exists(file))
	                    ShowFolderBrowser(false, file);	                
	            }
	        }
	    }

	    protected override void OnContentRendered(EventArgs e)
		{
			base.OnContentRendered(e);
			WindowState = mmApp.Configuration.WindowPosition.WindowState;
		}

		protected override void OnDeactivated(EventArgs e)
		{
			var editor = Model.ActiveEditor;
			if (editor != null)
			{
				var doc = Model.ActiveDocument;
				doc.IsActive = true;

				doc.LastEditorLineNumber = editor.GetLineNumber();
				if (doc.LastEditorLineNumber == -1)
					doc.LastEditorLineNumber = 0;
			}

			base.OnDeactivated(e);
			mmApp.SetWorkingSet(10000000, 5000000);
		}

		protected void OnActivated(object sender, EventArgs e)
		{
			var selectedTab = TabControl.SelectedItem as TabItem;

			// check for external file changes
			for (int i = TabControl.Items.Count - 1; i > -1; i--)
			{
				var tab = TabControl.Items[i] as TabItem;

				if (tab != null)
				{
					var editor = tab.Tag as MarkdownDocumentEditor;
					var doc = editor?.MarkdownDocument;
					if (doc == null)
						continue;

					if (doc.HasFileCrcChanged())
					{
						// force update to what's on disk so it doesn't fire again
						// do here prior to dialogs so this code doesn't fire recursively
						doc.UpdateCrc();

						string filename = doc.FilenamePathWithIndicator.Replace("*", "");
						string template = filename +
						                  "\r\n\r\nThis file has been modified by another program.\r\nDo you want to reload it?";

						if (MessageBox.Show(this, template,
							    "Reload",
							    MessageBoxButton.YesNo,
							    MessageBoxImage.Question) == MessageBoxResult.Yes)
						{
							if (!doc.Load(doc.Filename))
							{
								MessageBox.Show(this, "Unable to re-load current document.",
									"Error re-loading file",
									MessageBoxButton.OK, MessageBoxImage.Exclamation);
								continue;
							}

							try
							{
								dynamic pos = editor.AceEditor?.getscrolltop(false);
								editor.SetMarkdown(doc.CurrentText);
								editor.AceEditor?.updateDocumentStats(false);
								if (pos != null && pos > 0)
									editor.AceEditor?.setscrolltop(pos);
							}
							catch (Exception ex)
							{
								mmApp.Log("Changed file notification update failure", ex);
								MessageBox.Show(this, "Unable to re-load current document.",
									"Error re-loading file",
									MessageBoxButton.OK, MessageBoxImage.Exclamation);
							}

							if (tab == selectedTab)
								PreviewMarkdown(editor, keepScrollPosition: true);
						}
					}
				}

				// Ensure that user hasn't higlighted a MenuItem so the menu doesn't lose focus
				if (!MainMenu.Items.OfType<MenuItem>().Any(item => item.IsHighlighted))
				{
					var selectedEditor = selectedTab.Tag as MarkdownDocumentEditor;
					if (selectedEditor != null)
					{
						try
						{
							selectedEditor.WebBrowser.Focus();
							selectedEditor.SetEditorFocus();
							selectedEditor.RestyleEditor();
						}
						catch
						{
						}
					}
				}
			}
		}

	    protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			Hide();

			AddinManager.Current.RaiseOnApplicationShutdown();

			bool isNewVersion = CheckForNewVersion(false, false);

			mmApp.Configuration.ApplicationUpdates.AccessCount++;

			SaveSettings();

			if (!CloseAllTabs())
			{
				Show();
				e.Cancel = true;
				return;
			}

			Top -= 10000;

			if (mmApp.Configuration.UseSingleWindow)
			{
				PipeManager?.StopServer();

				if (App.Mutex != null)
					App.Mutex.Dispose();
			}

			var displayCount = 6;
			if (mmApp.Configuration.ApplicationUpdates.AccessCount > 250)
				displayCount = 1;
			else if (mmApp.Configuration.ApplicationUpdates.AccessCount > 100)
				displayCount = 2;
			else if (mmApp.Configuration.ApplicationUpdates.AccessCount > 50)
				displayCount = 4;
			
			if (!isNewVersion &&
			    mmApp.Configuration.ApplicationUpdates.AccessCount % displayCount == 0 &&
			    !UnlockKey.IsRegistered())
			{
				Hide();
				Top += 10000;
				var rd = new RegisterDialog();
				rd.Owner = this;
				rd.ShowDialog();
			}

			mmApp.Shutdown();

			e.Cancel = false;
		}

		public void AddRecentFile(string file, bool noConfigWrite = false)
		{
			Dispatcher.InvokeAsync(() =>
				{
					mmApp.Configuration.AddRecentFile(file);
					RecentDocumentsContextList();
					mmApp.Configuration.LastFolder = Path.GetDirectoryName(file);

					if (!noConfigWrite)
						mmApp.Configuration.Write();

					try
					{
						MostRecentlyUsedList.AddToRecentlyUsedDocs(Path.GetFullPath(file));
					}
					catch{}
				},DispatcherPriority.ApplicationIdle);
		}

		/// <summary>
		/// Creates the Recent Items Context list
		/// </summary>        
		private void RecentDocumentsContextList()
		{
			var context = Resources["ContextMenuRecentFiles"] as ContextMenu;
			if (context == null)
				return;

			context.Items.Clear();
			ButtonRecentFiles.Items.Clear();

			List<string> badFiles = new List<string>();
			foreach (string file in mmApp.Configuration.RecentDocuments)
			{
				if (!File.Exists(file))
				{
					badFiles.Add(file);
					continue;
				}
				var mi = new MenuItem()
				{
					Header = file,
				};

				mi.Click += (object s, RoutedEventArgs ev) =>
				{
					OpenTab(file, rebindTabHeaders: true);

					// TODO: Check this and make sure we get recent file added from tab of new tab selection
					//AddRecentFile(file);
				};
				context.Items.Add(mi);

				var mi2 = new MenuItem()
				{
					Header = file,
				};
				mi2.Click += (object s, RoutedEventArgs ev) => OpenTab(file, rebindTabHeaders: true);
				ButtonRecentFiles.Items.Add(mi2);
			}
			ToolbarButtonRecentFiles.ContextMenu = context;

			foreach (var file in badFiles)
				mmApp.Configuration.RecentDocuments.Remove(file);
		}

		void RestoreSettings()
		{
			var conf = mmApp.Configuration;

			if (conf.WindowPosition.Width != 0)
			{
				Left = conf.WindowPosition.Left;
				Top = conf.WindowPosition.Top;
				Width = conf.WindowPosition.Width;
				Height = conf.WindowPosition.Height;
			}


			if (mmApp.Configuration.RememberLastDocumentsLength > 0)
			{
				//var selectedDoc = conf.RecentDocuments.FirstOrDefault();
				TabItem selectedTab = null;

				string firstDoc = conf.RecentDocuments.FirstOrDefault();


				// prevent TabSelectionChanged to fire
				batchTabAction = true;

				// since docs are inserted at the beginning we need to go in reverse
				foreach (var doc in conf.OpenDocuments.Take(mmApp.Configuration.RememberLastDocumentsLength).Reverse())
				{
					if (doc.Filename == null)
						continue;

					if (File.Exists(doc.Filename))
					{
						var tab = OpenTab(doc.Filename, selectTab: false, batchOpen: true, initialLineNumber: doc.LastEditorLineNumber);
						if (tab == null)
							continue;

						if (doc.IsActive)
							selectedTab = tab;
					}
				}

				if (selectedTab != null)
					TabControl.SelectedItem = selectedTab;
				else
					TabControl.SelectedIndex = 0;

				batchTabAction = false;

			}

			Model.IsPreviewBrowserVisible = mmApp.Configuration.IsPreviewVisible;			

			ShowFolderBrowser(!mmApp.Configuration.FolderBrowser.Visible);

            // force background so we have a little more contrast
		    if (mmApp.Configuration.ApplicationTheme == Themes.Light)
		    {
		        ContentGrid.Background = (SolidColorBrush) new BrushConverter().ConvertFromString("#eee");		       
		        ToolbarPanelMain.Background = (SolidColorBrush)new BrushConverter().ConvertFromString("#D5DAE8");
            }
		    else
		        ContentGrid.Background = (SolidColorBrush) new BrushConverter().ConvertFromString("#333");
		}

		/// <summary>
		/// Save active settings of the UI that are persisted in the configuration
		/// </summary>
		public void SaveSettings()
		{
			var config = mmApp.Configuration;
			config.IsPreviewVisible = Model.IsPreviewBrowserVisible;
			config.WindowPosition.IsTabHeaderPanelVisible = true;

			if (WindowState == WindowState.Normal)
			{
				config.WindowPosition.Left = Convert.ToInt32(Left);
				config.WindowPosition.Top = Convert.ToInt32(Top);
				config.WindowPosition.Width = Convert.ToInt32(Width);
				config.WindowPosition.Height = Convert.ToInt32(Height);
				config.WindowPosition.SplitterPosition = Convert.ToInt32(MainWindowPreviewColumn.Width.Value);
			}

			if (WindowState != WindowState.Minimized)
				config.WindowPosition.WindowState = WindowState;

			if (FolderBrowserColumn.Width.Value > 20)
			{
				config.FolderBrowser.WindowWidth = Convert.ToInt32(FolderBrowserColumn.Width.Value);
				config.FolderBrowser.Visible = true;
			}
			else
				config.FolderBrowser.Visible = false;


			config.FolderBrowser.FolderPath = FolderBrowser.FolderPath;


			config.OpenDocuments.Clear();

			if (mmApp.Configuration.RememberLastDocumentsLength > 0)
			{
				var documents = TabControl.GetOrderedHeaders().Select(itm =>
				{
					var item = itm.Content as TabItem;
					var editor = item?.Tag as MarkdownDocumentEditor;

					if (editor == null)
						return null;

					return new {Editor = editor, Document = editor.MarkdownDocument};
				});

				foreach (var recentDocument in config.RecentDocuments.Take(mmApp.Configuration.RememberLastDocumentsLength))
				{
					var editor = getTabItemByFileName(recentDocument)?.Tag as MarkdownDocumentEditor;

					var doc = editor?.MarkdownDocument;
					if (doc == null)
						continue;

					doc.LastEditorLineNumber = editor.GetLineNumber();
					if (doc.LastEditorLineNumber < 0)
						doc.LastEditorLineNumber = 0;

					config.OpenDocuments.Add(doc);
				}

				//foreach (var item in TabControl.GetOrderedHeaders())
				//{
				//    var tab = item.Content as TabItem;
				//    var doc = tab.Tag as MarkdownDocumentEditor;
				//    if (doc != null)
				//        config.OpenDocuments.Add(doc.MarkdownDocument);
				//}
			}
			config.Write();
		}

		public bool SaveFile(bool secureSave = false)
		{
			var tab = TabControl.SelectedItem as TabItem;
			if (tab == null)
				return false;

			var md = tab.Content;
			var editor = tab.Tag as MarkdownDocumentEditor;
			var doc = editor?.MarkdownDocument;
			if (doc == null)
				return false;

            // prompt for password on a secure save
		    if (secureSave && editor.MarkdownDocument.Password == null)
		    {
		        var pwdDialog = new FilePasswordDialog(editor.MarkdownDocument,false);
		        pwdDialog.ShowDialog();
		    }

			if (!editor.SaveDocument())
			{
				//var res = await this.ShowMessageOverlayAsync("Unable to save Document",
				//    "Unable to save document most likely due to missing permissions.");

				MessageBox.Show("Unable to save document most likely due to missing permissions.",
					mmApp.ApplicationName);
				return false;
			}

			return true;
		}

		#endregion

		#region Tab Handling

	    /// <summary>
	    /// Opens a tab by a filename
	    /// </summary>
	    /// <param name="mdFile"></param>
	    /// <param name="editor"></param>
	    /// <param name="showPreviewIfActive"></param>
	    /// <param name="syntax"></param>
	    /// <param name="selectTab"></param>
	    /// <param name="rebindTabHeaders">
	    /// Rebinds the headers which should be done whenever a new Tab is
	    /// manually opened and added but not when opening in batch.
	    /// 
	    /// Checks to see if multiple tabs have the same filename open and
	    /// if so displays partial path.
	    /// 
	    /// New Tabs are opened at the front of the tab list at index 0
	    /// </param>
	    /// <returns></returns>
	    public TabItem OpenTab(string mdFile = null,
	        MarkdownDocumentEditor editor = null,
	        bool showPreviewIfActive = false,
	        string syntax = "markdown",
	        bool selectTab = true,
	        bool rebindTabHeaders = false,
	        bool batchOpen = false,
	        int initialLineNumber = 0)
	    {
	        if (mdFile != null && mdFile != "untitled" &&
	            (!File.Exists(mdFile) ||
	             !AddinManager.Current.RaiseOnBeforeOpenDocument(mdFile)))
	            return null;

	        var tab = new TabItem();

	        tab.Margin = new Thickness(0, 0, 3, 0);
	        tab.Padding = new Thickness(2, 0, 7, 2);
	        tab.Background = Background;

	        ControlsHelper.SetHeaderFontSize(tab, 13F);

	        var webBrowser = new WebBrowser
	        {
	            Visibility = Visibility.Hidden,
	            Margin = new Thickness(-4, 0, 0, 0)
	        };
	        tab.Content = webBrowser;


	        if (editor == null)
	        {
	            editor = new MarkdownDocumentEditor(webBrowser)
	            {
	                Window = this,
	                EditorSyntax = syntax,
	                InitialLineNumber = initialLineNumber
	            };

	            var doc = new MarkdownDocument()
	            {
	                Filename = mdFile ?? "untitled",
	                Dispatcher = Dispatcher
	            };
	            if (doc.Filename != "untitled")
	            {
	                doc.Filename = mmFileUtils.GetPhysicalPath(doc.Filename);

	                if (doc.HasBackupFile())
	                {
	                    try
	                    {
	                        ShowStatus("Auto-save recovery files have been found and opened in the editor.",
	                            milliSeconds: 9000);
	                        SetStatusIcon(FontAwesomeIcon.Warning, Colors.Red);
	                        {
	                            File.Copy(doc.BackupFilename, doc.BackupFilename + ".md");
	                            OpenTab(doc.BackupFilename + ".md");
	                            File.Delete(doc.BackupFilename + ".md");
	                        }
	                    }
	                    catch (Exception ex)
	                    {
	                        string msg = "Unable to open backup file: " + doc.BackupFilename + ".md";
	                        mmApp.Log(msg, ex);
	                        MessageBox.Show(
	                            "A backup file was previously saved, but we're unable to open it.\r\n" + msg,
	                            "Cannot open backup file",
	                            MessageBoxButton.OK,
	                            MessageBoxImage.Warning);
	                    }
	                }

	                if (doc.Password == null && doc.IsFileEncrypted())
	                {
	                    var pwdDialog = new FilePasswordDialog(doc, true)
	                    {
	                        Owner = this
	                    };
	                    bool? pwdResult = pwdDialog.ShowDialog();
	                    if (pwdResult == false)
	                    {
	                        ShowStatus("Encrypted document not opened, due to missing password.",
	                            mmApp.Configuration.StatusTimeout);

	                        return null;
	                    }
	                }


	                if (!doc.Load())
	                {
	                    if (!batchOpen)
	                    {
                            var msg = "Most likely you don't have access to the file";
	                        if (doc.Password != null && doc.IsFileEncrypted())
	                            msg = "Invalid password for opening this file";
	                        var file = Path.GetFileName(doc.Filename);

                            MessageBox.Show(
	                            $"{msg}.\r\n\r\n{file}",
	                            "Can't open File", MessageBoxButton.OK,
	                            MessageBoxImage.Warning);
	                    }

	                    return null;
	                }
	            }

	            doc.PropertyChanged += (sender, e) =>
	            {
	                if (e.PropertyName == "IsDirty")
	                {
	                    //CommandManager.InvalidateRequerySuggested();
	                    Model.SaveCommand.InvalidateCanExecute();
	                }
	            };
	            editor.MarkdownDocument = doc;

	            SetTabHeaderBinding(tab, doc, "FilenameWithIndicator");

	            tab.ToolTip = doc.Filename;
	        }

	        var filename = Path.GetFileName(editor.MarkdownDocument.Filename);
	        tab.Tag = editor;


	        editor.LoadDocument();

	        // is the tab already open?
	        TabItem existingTab = null;
	        if (filename != "untitled")
	        {
	            foreach (TabItem tb in TabControl.Items)
	            {
	                var lEditor = tb.Tag as MarkdownDocumentEditor;
	                if (lEditor.MarkdownDocument.Filename == editor.MarkdownDocument.Filename)
	                {
	                    existingTab = tb;
	                    break;
	                }
	            }
	        }

	        Model.OpenDocuments.Add(editor.MarkdownDocument);
	        Model.ActiveDocument = editor.MarkdownDocument;

	        if (existingTab != null)
	            TabControl.Items.Remove(existingTab);

	        tab.IsSelected = false;

	        TabControl.Items.Insert(0, tab);


	        if (selectTab)
	        {
	            TabControl.SelectedItem = tab;

	            if (showPreviewIfActive && PreviewBrowser.Width > 5)
	                PreviewMarkdown(); //Model.PreviewBrowserCommand.Execute(ButtonHtmlPreview);
	            SetWindowTitle();
	        }

	        AddinManager.Current.RaiseOnAfterOpenDocument(editor.MarkdownDocument);

	        if (rebindTabHeaders)
	            BindTabHeaders();


	        return tab;
	    }

	    /// <summary>
		/// Binds all Tab Headers
		/// </summary>        
		void BindTabHeaders()
		{
			var tabList = new List<TabItem>();
			foreach (TabItem tb in TabControl.Items)
				tabList.Add(tb);

			var tabItems = tabList
				.Select(tb => Path.GetFileName(((MarkdownDocumentEditor) tb.Tag).MarkdownDocument.Filename.ToLower()))
				.GroupBy(fn => fn)
				.Select(tbCol => new
				{
					Filename = tbCol.Key,
					Count = tbCol.Count()
				});

			foreach (TabItem tb in TabControl.Items)
			{
				var doc = ((MarkdownDocumentEditor) tb.Tag).MarkdownDocument;

				if (tabItems.Any(ti => ti.Filename == Path.GetFileName(doc.Filename.ToLower()) &&
				                       ti.Count > 1))

					SetTabHeaderBinding(tb, doc, "FilenamePathWithIndicator");
				else
					SetTabHeaderBinding(tb, doc, "FilenameWithIndicator");
			}
		}

		/// <summary>
		/// Binds the tab header to an expression
		/// </summary>
		/// <param name="tab"></param>   
		/// <param name="document"></param>     
		/// <param name="propertyPath"></param>
		private void SetTabHeaderBinding(TabItem tab, MarkdownDocument document,
			string propertyPath = "FilenameWithIndicator")
		{
			if (document == null || tab == null)
				return;

			try
			{
				var headerBinding = new Binding
				{
					Source = document,
					Path = new PropertyPath(propertyPath),
					Mode = BindingMode.OneWay
				};
				BindingOperations.SetBinding(tab, HeaderedContentControl.HeaderProperty, headerBinding);
			}
			catch (Exception ex)
			{
				mmApp.Log("SetTabHeaderBinding Failed. Assigning explicit path", ex);
				tab.Header = document.FilenameWithIndicator;
			}
		}

		/// <summary>
		///  Flag used to let us know we don't want to perform tab selection operations
		/// </summary>
		private bool batchTabAction = false;

		private bool CloseAllTabs(TabItem allExcept = null)
		{
			batchTabAction = true;
			for (int i = TabControl.Items.Count - 1; i > -1; i--)
			{
				var tab = TabControl.Items[i] as TabItem;

				if (tab != null)
				{
					if (allExcept != null && tab == allExcept)
						continue;

					if (!CloseTab(tab, rebindTabHeaders: false))
						return false;
				}
			}
			batchTabAction = false;
			return true;
		}

		/// <summary>
		/// Closes a tab and ask for confirmation if the tab doc 
		/// is dirty.
		/// </summary>
		/// <param name="tab"></param>
		/// <param name="rebindTabHeaders">
		/// When true tab headers are rebound to handle duplicate filenames
		/// with path additions.
		/// </param>
		/// <returns>true if tab can close, false if it should stay open</returns>
		public bool CloseTab(TabItem tab, bool rebindTabHeaders = true)
		{
			var editor = tab?.Tag as MarkdownDocumentEditor;
			if (editor == null)
				return false;

			bool returnValue = true;

            tab.Background = Brushes.Green;
            tab.Padding = new Thickness(200);

			var doc = editor.MarkdownDocument;

			doc.CleanupBackupFile();

			if (doc.IsDirty)
			{
				var res = MessageBox.Show(Path.GetFileName(doc.Filename) + "\r\n\r\nhas been modified.\r\n" +
				                          "Do you want to save changes?",
					"Save Document",
					MessageBoxButton.YesNoCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);
				if (res == MessageBoxResult.Cancel)
				{
					return false; // don't close
				}
				if (res == MessageBoxResult.No)
				{
					// close but don't save 
				}
				else
				{
					if (doc.Filename == "untitled")
						Model.SaveAsCommand.Execute(ButtonSaveAsFile);
					else if (!SaveFile())
						returnValue = false;
				}
			}

			doc.LastEditorLineNumber = editor.GetLineNumber();
			if (doc.LastEditorLineNumber == -1)
				doc.LastEditorLineNumber = 0;

			tab.Tag = null;
			TabControl.Items.Remove(tab);

			if (TabControl.Items.Count == 0)
			{
				PreviewBrowser.Visibility = Visibility.Hidden;
				PreviewBrowser.Navigate("about:blank");
				Model.ActiveDocument = null;
				Title = "Markdown Monster" +
				        (UnlockKey.Unlocked ? "" : " (unregistered)");
			}

			if (rebindTabHeaders)
				BindTabHeaders();

			return returnValue; // close
		}

		/// <summary>
		/// Closes a tab and ask for confirmation if the tab doc 
		/// is dirty.
		/// </summary>
		/// <param name="filename">
		/// The absolute path to the file opened in the tab that 
		/// is going to be closed
		/// </param>
		/// <returns>true if tab can close, false if it should stay open or 
		/// filename not opened in any tab</returns>
		public bool CloseTab(string filename)
		{
			TabItem tab = getTabItemByFileName(filename);

			if (tab != null)
			{
				return CloseTab(tab);
			}
			else
			{
				return false;
			}
		}

		private TabItem getTabItemByFileName(string filename)
		{
			TabItem tab = null;
			foreach (TabItem tabItem in TabControl.Items.Cast<TabItem>())
			{
				var markdownEditor = tabItem.Tag as MarkdownDocumentEditor;
				if (markdownEditor.MarkdownDocument.Filename.Equals(filename))
				{
					tab = tabItem;
					break;
				}
			}
			return tab;
		}

		private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (batchTabAction)
				return;

			var editor = GetActiveMarkdownEditor();
			if (editor == null)
				return;

			if (mmApp.Configuration.IsPreviewVisible)
				PreviewMarkdown();

			SetWindowTitle();

			foreach (var doc in Model.OpenDocuments)
				doc.IsActive = false;

			Model.ActiveDocument = editor.MarkdownDocument;
			Model.ActiveDocument.IsActive = true;

			AddRecentFile(Model.ActiveDocument?.Filename, noConfigWrite: true);

			AddinManager.Current.RaiseOnDocumentActivated(Model.ActiveDocument);

			Model.ActiveEditor.RestyleEditor();

			editor.WebBrowser.Focus();
			editor.SetEditorFocus();
		}


		private void TabControlDragablz_TabItemClosing(ItemActionCallbackArgs<TabablzControl> e)
		{
			var tab = e.DragablzItem.DataContext as TabItem;
			if (tab == null)
				return;

			if (!CloseTab(tab))
				e.Cancel();
		}

		/// <summary>
		/// Sets the Window Title followed by Markdown Monster (registration status)
		/// by default the filename is used and it's updated whenever tabs are changed.
		/// 
		/// Generally just call this when you need to have the title updated due to
		/// file name change that doesn't change the active tab.
		/// </summary>
		/// <param name="title"></param>
		public void SetWindowTitle(string title = null)
		{
			if (title == null)
			{
				var editor = GetActiveMarkdownEditor();
				if (editor == null)
					return;
				title = editor.MarkdownDocument.FilenameWithIndicator.Replace("*", "");
			}

			Title = title +
			        "  - Markdown Monster" +
			        (UnlockKey.Unlocked ? "" : " (unregistered)");
		}

		#endregion

		#region Worker Functions

		/// <summary>
		/// Shows or hides the preview browser
		/// </summary>
		/// <param name="hide"></param>
		public void ShowPreviewBrowser(bool hide = false, bool refresh = false)
		{
			if (!hide)
			{
				PreviewBrowser.Visibility = Visibility.Visible;

				MainWindowSeparatorColumn.Width = new GridLength(12);
				if (!refresh)
				{
					if (mmApp.Configuration.WindowPosition.SplitterPosition < 100)
						mmApp.Configuration.WindowPosition.SplitterPosition = 600;

					if (!Model.IsPresentationMode)
						MainWindowPreviewColumn.Width =
							new GridLength(mmApp.Configuration.WindowPosition.SplitterPosition);
				}
			}
			else
			{
				if (MainWindowPreviewColumn.Width.Value > 100)
					mmApp.Configuration.WindowPosition.SplitterPosition =
						Convert.ToInt32(MainWindowPreviewColumn.Width.Value);

				MainWindowSeparatorColumn.Width = new GridLength(0);
				MainWindowPreviewColumn.Width = new GridLength(0);

				PreviewBrowser.Navigate("about:blank");
			}
		}

		/// <summary>
		/// Shows or hides the File Browser
		/// </summary>
		/// <param name="hide"></param>
		public void ShowFolderBrowser(bool hide = false, string folder = null)
		{
			if (hide)
			{
				if (FolderBrowserColumn.Width.Value > 20)
					mmApp.Configuration.FolderBrowser.WindowWidth = Convert.ToInt32(FolderBrowserColumn.Width.Value);

				FolderBrowserColumn.Width = new GridLength(0);
				FolderBrowserSeparatorColumn.Width = new GridLength(0);
                
    			mmApp.Configuration.FolderBrowser.Visible = false;
			}
			else
			{
			    if (folder == null)
			        folder = FolderBrowser.FolderPath;

			    Dispatcher.InvokeAsync(() =>
			    {
			        if (string.IsNullOrEmpty(folder) && Model.ActiveDocument != null)
			            folder = Path.GetDirectoryName(Model.ActiveDocument.Filename);

			        FolderBrowser.FolderPath = folder;
			    });

				FolderBrowserColumn.Width = new GridLength(mmApp.Configuration.FolderBrowser.WindowWidth);
				FolderBrowserSeparatorColumn.Width = new GridLength(14);
				mmApp.Configuration.FolderBrowser.Visible = true;
			}
		}

		// IMPORTANT: for browser COM CSE errors which can happen with script errors
		[HandleProcessCorruptedStateExceptions]
		[MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
		public void PreviewMarkdown(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false,
			bool showInBrowser = false)
		{
			try
			{
				// only render if the preview is actually visible and rendering in Preview Browser
				if (!Model.IsPreviewBrowserVisible && !showInBrowser)
					return;

				if (editor == null)
					editor = GetActiveMarkdownEditor();

				if (editor == null)
					return;

				var doc = editor.MarkdownDocument;
				var ext = Path.GetExtension(doc.Filename).ToLower().Replace(".", "");

				string renderedHtml = null;

				if (string.IsNullOrEmpty(ext) || ext == "md" || ext=="mdcrypt" || ext == "markdown" || ext == "html" || ext == "htm")
				{
					dynamic dom = null;
					if (!showInBrowser)
					{
						if (keepScrollPosition)
						{
							dom = PreviewBrowser.Document;
							editor.MarkdownDocument.LastEditorLineNumber = dom.documentElement.scrollTop;
						}
						else
						{
							ShowPreviewBrowser(false, false);
							editor.MarkdownDocument.LastEditorLineNumber = 0;
						}
					}

					if (ext == "html" || ext == "htm")
					{
						if (!editor.MarkdownDocument.WriteFile(editor.MarkdownDocument.HtmlRenderFilename,
								editor.MarkdownDocument.CurrentText))
							// need a way to clear browser window
							return;
					}
					else
					{
					    bool usePragma = !showInBrowser && mmApp.Configuration.PreviewSyncMode != PreviewSyncMode.None;
                        renderedHtml = editor.MarkdownDocument.RenderHtmlToFile(usePragmaLines: usePragma,
							            renderLinksExternal: mmApp.Configuration.MarkdownOptions.RenderLinksAsExternal);
						if (renderedHtml == null)
						{
							SetStatusIcon(FontAwesomeIcon.Warning, Colors.Red, false);
							ShowStatus($"Access denied: {Path.GetFileName(editor.MarkdownDocument.Filename)}", 5000);
							// need a way to clear browser window

							return;
						}

						renderedHtml = StringUtils.ExtractString(renderedHtml,
							"<!-- Markdown Monster Content -->",
							"<!-- End Markdown Monster Content -->");
					}

					if (showInBrowser)
					{
					    var url = editor.MarkdownDocument.HtmlRenderFilename;
					    mmFileUtils.ShowExternalBrowser(url);
					    return;
					}
					else
					{
						PreviewBrowser.Cursor = Cursors.None;
						PreviewBrowser.ForceCursor = true;

						// if content contains <script> tags we must do a full page refresh
						bool forceRefresh = renderedHtml != null && renderedHtml.Contains("<script ");


						if (keepScrollPosition && !mmApp.Configuration.AlwaysUsePreviewRefresh && !forceRefresh)
						{
							string browserUrl = PreviewBrowser.Source.ToString().ToLower();
							string documentFile = "file:///" +
							                      editor.MarkdownDocument.HtmlRenderFilename.Replace('\\', '/')
								                      .ToLower();
							if (browserUrl == documentFile)
							{
								dom = PreviewBrowser.Document;
								//var content = dom.getElementById("MainContent");


								if (string.IsNullOrEmpty(renderedHtml))
									PreviewMarkdown(editor, false, false); // fully reload document
								else
								{
									try
									{
										// explicitly update the document with JavaScript code
										// much more efficient and non-jumpy and no wait cursor
										var window = dom.parentWindow;
										window.updateDocumentContent(renderedHtml);

										try
										{
											// scroll preview to selected line
											if (mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorAndPreview ||
											    mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorToPreview)
											{
												int lineno = editor.GetLineNumber();
												if (lineno > -1)
													window.scrollToPragmaLine(lineno);
											}
										}
										catch
										{
/* ignore scroll error */
										}
									}
									catch
									{
										// Refresh doesn't fire Navigate event again so 
										// the page is not getting initiallized properly
										//PreviewBrowser.Refresh(true);
										PreviewBrowser.Tag = "EDITORSCROLL";
										PreviewBrowser.Navigate(new Uri(editor.MarkdownDocument.HtmlRenderFilename));
									}
								}

								return;
							}
						}

						PreviewBrowser.Tag = "EDITORSCROLL";
						PreviewBrowser.Navigate(new Uri(editor.MarkdownDocument.HtmlRenderFilename));
						return;
					}
				}

				// not a markdown or HTML document to preview
				ShowPreviewBrowser(true, keepScrollPosition);
			}
			catch (Exception ex)
			{
				mmApp.Log("PreviewMarkdown failed (Exception captured - continuing)", ex);
			}
		}


		public void PreviewMarkdownAsync(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false)
		{
			if (!mmApp.Configuration.IsPreviewVisible)
				return;

			var current = DateTime.UtcNow;

			// prevent multiple stacked refreshes
			if (invoked == DateTime.MinValue) // || current.Subtract(invoked).TotalMilliseconds > 4000)
			{
				invoked = current;

				Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
					new Action(() =>
					{
						try
						{
							PreviewMarkdown(editor, keepScrollPosition);
						}
						catch (Exception ex)
						{
							Console.WriteLine("Preview Markdown Async Exception: " + ex.Message);
						}
						finally
						{
							invoked = DateTime.MinValue;
						}
					}));
			}
		}


		public MarkdownDocumentEditor GetActiveMarkdownEditor()
		{
			var tab = TabControl?.SelectedItem as TabItem;
			return tab?.Tag as MarkdownDocumentEditor;
		}

		bool CheckForNewVersion(bool force, bool closeForm = true, int timeout = 2000)
		{
			var updater = new ApplicationUpdater(typeof(MainWindow));
			bool isNewVersion = updater.IsNewVersionAvailable(!force, timeout: timeout);
			if (isNewVersion)
			{
				var res = MessageBox.Show(updater.VersionInfo.Detail + "\r\n\r\n" +
				                          "Do you want to download and install this version?",
					updater.VersionInfo.Title,
					MessageBoxButton.YesNo,
					MessageBoxImage.Information);

				if (res == MessageBoxResult.Yes)
				{
					ShellUtils.GoUrl(mmApp.Urls.InstallerDownloadUrl);

					if (closeForm)
						Close();
				}
			}
			mmApp.Configuration.ApplicationUpdates.LastUpdateCheck = DateTime.UtcNow.Date;

			return isNewVersion;
		}

		/// <summary>
		/// Check to see if the window is visible in the bounds of the 
		/// virtual screen space. If not adjust to main monitor off 0 position.
		/// </summary>
		/// <returns></returns>
		void FixMonitorPosition()
		{
			var virtualScreenHeight = SystemParameters.VirtualScreenHeight;
			var virtualScreenWidth = SystemParameters.VirtualScreenWidth;

			
			if (Left > virtualScreenWidth - 150)
				Left = 20;
			if (Top > virtualScreenHeight - 150)
				Top = 20;

			if (Left < SystemParameters.VirtualScreenLeft)
				Left = SystemParameters.VirtualScreenLeft;
			if (Top < SystemParameters.VirtualScreenTop)
				Top = SystemParameters.VirtualScreenTop;

			if (Width > virtualScreenWidth)
				Width = virtualScreenWidth - 40;
			if (Height > virtualScreenHeight)
				Height = virtualScreenHeight - 40;			
		}

		#endregion

		#region Button Handlers

		/// <summary>
		/// Generic button handler that handles a number of simple
		/// tasks in a single method to minimize class noise.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void Button_Handler(object sender, RoutedEventArgs e)
		{
			var button = sender;
			if (button == null )
				return;

			if (button == ButtonOpenFromHtml)
			{
				var fd = new OpenFileDialog
				{
					DefaultExt = ".htm",
					Filter = "Html files (*.htm,*.html)|*.htm;*.html|" +
					         "All files (*.*)|*.*",
					CheckFileExists = true,
					RestoreDirectory = true,
					Multiselect = true,
					Title = "Open Html as Markdown"
				};

				if (!string.IsNullOrEmpty(mmApp.Configuration.LastFolder))
					fd.InitialDirectory = mmApp.Configuration.LastFolder;

				var res = fd.ShowDialog();
				if (res == null || !res.Value)
					return;

				var html = File.ReadAllText(fd.FileName);

				var markdown = MarkdownUtilities.HtmlToMarkdown(html);

				OpenTab("untitled");
				var editor = GetActiveMarkdownEditor();
				editor.MarkdownDocument.CurrentText = markdown;
				PreviewMarkdown();
			}
			else if (button == ButtonNewWeblogPost)
			{
				AddinManager.Current.RaiseOnNotifyAddin("newweblogpost", null);
			}
			else if (button == ButtonExit)
			{
				Close();
			}
			else if (button == MenuAddinManager)
			{
				var form = new AddinManagerWindow();
				form.Owner = this;
				form.Show();
			}
			else if (button == MenuOpenConfigFolder)
			{
				ShellUtils.GoUrl(mmApp.Configuration.CommonFolder);
			}
			else if (button == MenuOpenPreviewFolder)
			{
				ShellUtils.GoUrl(Path.Combine(Environment.CurrentDirectory, "PreviewThemes",
					mmApp.Configuration.RenderTheme));
			}
			else if (button == MenuMarkdownMonsterSite)
			{
				ShellUtils.GoUrl(mmApp.Urls.WebSiteUrl);
			}
			else if (button == MenuBugReport)
			{
				ShellUtils.GoUrl(mmApp.Urls.SupportUrl);
			}
			else if (button == MenuCheckNewVersion)
			{
				ShowStatus("Checking for new version...");
				if (!CheckForNewVersion(true, timeout: 5000))
				{
					ShowStatus("Your version of Markdown Monster is up to date.", 6000);
					SetStatusIcon(FontAwesomeIcon.Check, Colors.Green);

					MessageBox.Show(
						"Your version of Markdown Monster is v" + mmApp.GetVersion() + " and you are up to date.",
						mmApp.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			else if (button == MenuRegister)
			{
				Window rf = new RegistrationForm();
				rf.Owner = this;
				rf.ShowDialog();
			}
			else if (button == ButtonAbout)
			{
				Window about = new About();
				about.Owner = this;
				about.Show();
			}
			else if (button == Button_Find)
			{
				var editor = GetActiveMarkdownEditor();
				if (editor == null)
					return;
				editor.ExecEditorCommand("find");
			}
			else if (button == Button_FindNext)
			{
				var editor = GetActiveMarkdownEditor();
				if (editor == null)
					return;
				editor.ExecEditorCommand("findnext");
			}
			else if (button == Button_Replace)
			{
				var editor = GetActiveMarkdownEditor();
				if (editor == null)
					return;
				editor.ExecEditorCommand("replace");
			}
			else if (button == ButtonScrollBrowserDown)
			{
				var editor = GetActiveMarkdownEditor();
				if (editor == null)
					return;
				editor.SpecialKey("ctrl-shift-down");
			}
			else if (button == ButtonScrollBrowserUp)
			{
				var editor = GetActiveMarkdownEditor();
				if (editor == null)
					return;
				editor.SpecialKey("ctrl-shift-d");
			}
            else if (button == ButtonWordWrap)
			{
			    Model.ActiveEditor?.SetWordWrap(Model.Configuration.EditorWrapText);
			}
            else if (button == ButtonLineNumbers)
			{   Model.ActiveEditor?.SetShowLineNumbers(Model.Configuration.EditorShowLineNumbers);
			}
            else if (button == ButtonStatusEncrypted)
			{
			    var dialog = new FilePasswordDialog(Model.ActiveDocument,false)
			    {
			        Owner = this
			    };
			    dialog.ShowDialog();
			}
			//else if (button == ButtonRefreshBrowser)
			//{
			//	var editor = GetActiveMarkdownEditor();
			//	if (editor == null)
			//		return;

			//	this.PreviewMarkdownAsync();
			//}
			else if (button == MenuDocumentation)
				ShellUtils.GoUrl(mmApp.Urls.DocumentationBaseUrl);
			else if (button == MenuMarkdownBasics)
				ShellUtils.GoUrl(mmApp.Urls.DocumentationBaseUrl + "_4ne1eu2cq.htm");
			else if (button == MenuCreateAddinDocumentation)
				ShellUtils.GoUrl(mmApp.Urls.DocumentationBaseUrl + "_4ne0s0qoi.htm");
			else if (button == MenuShowSampleDocument)
				OpenTab(Path.Combine(Environment.CurrentDirectory, "SampleMarkdown.md"));
			else if (button == MenuShowErrorLog)
			{
				string logFile = Path.Combine(mmApp.Configuration.CommonFolder, "MarkdownMonsterErrors.txt");
				if (File.Exists(logFile))
					ShellUtils.GoUrl(logFile);
				else
					MessageBox.Show("There are no errors in your log file.",
						mmApp.ApplicationName,
						MessageBoxButton.OK,
						MessageBoxImage.Information);
			}
		}


		private void ButtonCloseAllTabs_Click(object sender, RoutedEventArgs e)
		{
			TabItem except = null;

			var menuItem = sender as MenuItem;
			if (menuItem != null && menuItem.Name == "MenuCloseAllButThisTab")
				except = TabControl.SelectedItem as TabItem;

			CloseAllTabs(except);
			BindTabHeaders();
		}

		private void ButtonSpellCheck_Click(object sender, RoutedEventArgs e)
		{
			foreach (TabItem tab in TabControl.Items)
			{
				var editor = tab.Tag as MarkdownDocumentEditor;
				editor?.RestyleEditor();
			}
		}

		

		private void Button_CommandWindow(object sender, RoutedEventArgs e)
		{
			var editor = GetActiveMarkdownEditor();
			if (editor == null)
				return;

			
			string path = Path.GetDirectoryName(editor.MarkdownDocument.Filename);
			mmFileUtils.OpenTerminal(path);
		}
        
	    private void Button_OpenExplorer(object sender, RoutedEventArgs e)
		{
			var editor = GetActiveMarkdownEditor();
			if (editor == null)
				return;
			Process.Start("explorer.exe", "/select,\"" + editor.MarkdownDocument.Filename + "\"");
		}

		private void Button_CopyFoldername(object sender, RoutedEventArgs e)
		{
			var editor = GetActiveMarkdownEditor();
			if (editor == null)
				return;

			if (editor.MarkdownDocument.Filename == "untitled")
				return;

			string path = Path.GetDirectoryName(editor.MarkdownDocument.Filename);
			Clipboard.SetText(path);
		}

		internal void Button_PasteMarkdownFromHtml(object sender, RoutedEventArgs e)
		{
			var editor = GetActiveMarkdownEditor();
			if (editor == null)
				return;

			string html = null;
			if (Clipboard.ContainsText(TextDataFormat.Html))
				html = Clipboard.GetText(TextDataFormat.Html);

			if (!string.IsNullOrEmpty(html))
				html = StringUtils.ExtractString(html, "<!--StartFragment-->", "<!--EndFragment-->");
			else
				html = Clipboard.GetText();

			if (string.IsNullOrEmpty(html))
				return;

			var markdown = MarkdownUtilities.HtmlToMarkdown(html);

			editor.SetSelection(markdown);
			editor.SetEditorFocus();
			PreviewMarkdownAsync(editor, true);
		}

		internal void Button_CopyMarkdownAsHtml(object sender, RoutedEventArgs e)
		{
			var editor = GetActiveMarkdownEditor();
			if (editor == null)
				return;

			var markdown = editor.GetSelection();
			var html = editor.RenderMarkdown(markdown);

			if (!string.IsNullOrEmpty(html))
			{
				Clipboard.SetText(html);
				ShowStatus("Html has been pasted to the clipboard.", 4000);
			}
			editor.SetEditorFocus();
			editor.Window.PreviewMarkdownAsync();
		}

		#endregion

		#region Miscelleaneous Events

		/// <summary>
		/// Handle drag and drop of file. Note only works when dropped on the
		/// window - doesn't not work when dropped on the editor.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void MainWindow_Drop(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[]) e.Data.GetData(DataFormats.FileDrop);

				foreach (var file in files)
				{
					var ext = Path.GetExtension(file.ToLower());
					if (File.Exists(file) && mmApp.AllowedFileExtensions.Contains($",{ext},"))
					{
						OpenTab(file, rebindTabHeaders: true);
					}
				}
			}
		}

		private void PreviewBrowser_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (e.NewSize.Width > 100)
			{
				int width = Convert.ToInt32(MainWindowPreviewColumn.Width.Value);
				if (width > 100)
					mmApp.Configuration.WindowPosition.SplitterPosition = width;
			}
		}

		private void EditorTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			foreach (TabItem tab in TabControl.Items)
			{
				var editor = tab.Tag as MarkdownDocumentEditor;
				editor?.RestyleEditor();
			}

			PreviewMarkdownAsync();
		}

		private void RenderTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			PreviewMarkdownAsync();
		}

		private void MarkdownParserName_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (mmApp.Configuration != null && !string.IsNullOrEmpty(mmApp.Configuration.MarkdownOptions.MarkdownParserName))
			{
				MarkdownParserFactory.GetParser(parserAddinId: mmApp.Configuration.MarkdownOptions.MarkdownParserName,
					forceLoad: true);
				PreviewMarkdownAsync();
			}
		}


		private void HandleNamedPipe_OpenRequest(string filesToOpen)
		{		 
            Dispatcher.Invoke(() =>
			{
				if (!string.IsNullOrEmpty(filesToOpen))
				{
                    var parms = StringUtils.GetLines(filesToOpen.Trim());
                    
				    OpenFilesFromCommandLine(parms);


					//TabItem lastTab = null;
					//foreach (var file in parms)
					//{
					//    if (!string.IsNullOrEmpty(file))
					//    {
					//        var ext = Path.GetExtension(file);

					//        if (string.IsNullOrEmpty(ext))					        
					//            ShowFolderBrowser(false, file);					        
     //                       else
     //                           lastTab = OpenTab(file.Trim());                            
					//    }
					//}
					//if (lastTab != null)
					//	Dispatcher.InvokeAsync(() => TabControl.SelectedItem = lastTab);
					//BindTabHeaders();
				}

				Topmost = true;

				if (WindowState == WindowState.Minimized)
					WindowState = WindowState.Normal;

				Activate();

				// restore out of band
				Dispatcher.BeginInvoke(new Action(() => { Topmost = false; }),DispatcherPriority.ApplicationIdle);
			});
		}

        #endregion

        #region StatusBar Display

        DebounceDispatcher debounce = new DebounceDispatcher();

		public void ShowStatus(string message = null, int milliSeconds = 0)
		{
			if (message == null)
			{
				message = "Ready";
				SetStatusIcon();
			}

			StatusText.Text = message;
            
			if (milliSeconds > 0)
			{
                debounce.Debounce(milliSeconds,(win) =>
                {
                        var window = win as MainWindow;
                        window.ShowStatus(null, 0);
                }, this);
                //            Dispatcher.DelayWithPriority(milliSeconds, (win) =>
                //{
                //	var window = win as MainWindow;
                //	window.ShowStatus(null, 0);
                //}, this);
            }
			WindowUtilities.DoEvents();
		}

		/// <summary>
		/// Status the statusbar icon on the left bottom to some indicator
		/// </summary>
		/// <param name="icon"></param>
		/// <param name="color"></param>
		/// <param name="spin"></param>
		public void SetStatusIcon(FontAwesomeIcon icon, Color color, bool spin = false)
		{
			StatusIcon.Icon = icon;
			StatusIcon.Foreground = new SolidColorBrush(color);
			if (spin)
				StatusIcon.SpinDuration = 30;

			StatusIcon.Spin = spin;
		}

		/// <summary>
		/// Resets the Status bar icon on the left to its default green circle
		/// </summary>
		public void SetStatusIcon()
		{
			StatusIcon.Icon = FontAwesomeIcon.Circle;
			StatusIcon.Foreground = new SolidColorBrush(Colors.Green);
			StatusIcon.Spin = false;
			StatusIcon.SpinDuration = 0;
			StatusIcon.StopSpin();
		}

		/// <summary>
		/// Helper routine to show a Metro Dialog. Note this dialog popup is fully async!
		/// </summary>
		/// <param name="title"></param>
		/// <param name="message"></param>
		/// <param name="style"></param>
		/// <param name="settings"></param>
		/// <returns></returns>
		public async Task<MessageDialogResult> ShowMessageOverlayAsync(string title, string message,
			MessageDialogStyle style = MessageDialogStyle.Affirmative,
			MetroDialogSettings settings = null)
		{
			return await this.ShowMessageAsync(title, message, style, settings);
		}

		#endregion

		#region Preview Browser Operation

		private void InitializePreviewBrowser()
		{
			// wbhandle has additional browser initialization code
			// using the WebBrowserHostUIHandler
			PreviewBrowser.LoadCompleted += PreviewBrowserOnLoadCompleted;
			//PreviewBrowser.Navigate("about:blank");            
		}


		private void PreviewBrowserOnLoadCompleted(object sender, NavigationEventArgs e)
		{
			string url = e.Uri.ToString();
			if (!url.Contains("_MarkdownMonster_Preview"))
				return;

			bool shouldScrollToEditor = PreviewBrowser.Tag != null && PreviewBrowser.Tag.ToString() == "EDITORSCROLL";
			PreviewBrowser.Tag = null;

			dynamic window = null;
			MarkdownDocumentEditor editor = null;
			try
			{
				editor = GetActiveMarkdownEditor();
				dynamic dom = PreviewBrowser.Document;
				window = dom.parentWindow;
				dom.documentElement.scrollTop = editor.MarkdownDocument.LastEditorLineNumber;

				window.initializeinterop(editor);

				if (shouldScrollToEditor)
				{
					try
					{
						// scroll preview to selected line
						if (mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorAndPreview ||
						    mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorToPreview)
						{
							int lineno = editor.GetLineNumber();
							if (lineno > -1)
								window.scrollToPragmaLine(lineno);
						}
					}
					catch
					{
						/* ignore scroll error */
					}
				}
			}
			catch
			{
				// try again
				Task.Delay(500).ContinueWith(t =>
				{
					try
					{
						window.initializeinterop(editor);
					}
					catch
					{
						//mmApp.Log("Preview InitializeInterop failed: " + url, ex);
					}
				});
			}
		}

		#endregion
	}
}