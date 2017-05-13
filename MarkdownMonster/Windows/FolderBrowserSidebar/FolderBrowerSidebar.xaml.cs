using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MarkdownMonster.Annotations;
using Microsoft.WindowsAPICodePack.Dialogs;
using Westwind.Utilities;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using UserControl = System.Windows.Controls.UserControl;

namespace MarkdownMonster.Windows
{
	/// <summary>
	/// Interaction logic for FolderBrowerSidebar.xaml
	/// </summary>
	public partial class FolderBrowerSidebar : UserControl, INotifyPropertyChanged
	{

		public string FolderPath
		{
			get { return _folderPath; }
			set
			{
				if (value == _folderPath) return;
				
				if (string.IsNullOrEmpty(value))
					ActivePathItem = new PathItem();
				else if (value != _folderPath)
					SetTreeFromFolder(value, _folderPath != null);

				_folderPath = value;

				OnPropertyChanged(nameof(FolderPath));
				OnPropertyChanged(nameof(ActivePathItem));
			}
		}
		private string _folderPath;
		

		public PathItem ActivePathItem
		{
			get { return _activePath; }
			set
			{
				if (Equals(value, _activePath)) return;
				_activePath = value;
				OnPropertyChanged();
			}
		}
		private PathItem _activePath;

		public string SelectedFile
		{
			get { return (string)GetValue(SelectedFileProperty); }
			set { SetValue(SelectedFileProperty, value); }
		}

		// Using a DependencyProperty as the backing store for SelectedFile.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SelectedFileProperty =
			DependencyProperty.Register("SelectedFile", typeof(string), typeof(FolderBrowerSidebar), new PropertyMetadata(null));

		public event Action<object, EventArgs> FileSelected;


		private FolderStructure FolderStructure { get; set; } = new FolderStructure();

		#region Initialization
		public FolderBrowerSidebar()
		{
			InitializeComponent();
			Loaded += FolderBrowerSidebar_Loaded;

			
		}
		

		private void FolderBrowerSidebar_Loaded(object sender, RoutedEventArgs e)
		{
			var context = Resources["FileContextMenu"] as ContextMenu;
			context.DataContext = TreeFolderBrowser;

			DataContext = this;			
		}
		#endregion

		#region Folder Button and Text Handling

		private void ButtonUseCurrentFolder_Click(object sender, RoutedEventArgs e)
		{
			var doc = mmApp.Model.ActiveDocument;
			if (doc != null)
				FolderPath = System.IO.Path.GetDirectoryName(doc.Filename);

			TreeFolderBrowser.Focus();
		}

		private void ButtonSelectFolder_Click(object sender, RoutedEventArgs e)
		{
			string folder = FolderPath;

			if (string.IsNullOrEmpty(folder))
			{
				folder = mmApp.Model.ActiveDocument?.Filename;
				if (string.IsNullOrEmpty(folder))
					folder = System.IO.Path.GetDirectoryName(folder);
				else
					folder = KnownFolders.GetPath(KnownFolder.Libraries);
			}

			var dlg = new CommonOpenFileDialog();

			dlg.Title = "Select folder to open in the Folder Browser";
			dlg.IsFolderPicker = true;
			dlg.InitialDirectory = folder;
			dlg.RestoreDirectory = true;
			dlg.ShowHiddenItems = true;
			dlg.ShowPlacesList = true;
			dlg.EnsurePathExists = true;

			var result = dlg.ShowDialog();

			if (result != CommonFileDialogResult.Ok)
				return;

			FolderPath = dlg.FileName;

			TreeFolderBrowser.Focus();
		}

		private void ButtonRefreshFolder_Click(object sender, RoutedEventArgs e)
		{
			if (ActivePathItem != null)
			{
				ActivePathItem.Files.Clear();
				ActivePathItem.FullPath = FolderPath;
				ActivePathItem.IsFolder = true;
			}

			SetTreeFromFolder(FolderPath,true);			
		}

		private void ButtonClosePanel_Click(object sender, RoutedEventArgs e)
		{
			mmApp.Model.Window.ShowFolderBrowser(hide: true);
			mmApp.Model.ActiveEditor?.SetEditorFocus();
		}

		private void SetTreeFromFolder(string folder, bool setFocus = false)
		{
			Dispatcher.InvokeAsync(() =>
			{				
				ActivePathItem = FolderStructure.GetFilesAndFolders(folder);
				WindowUtilities.DoEvents();				

				if (TreeFolderBrowser.HasItems)
					SetTreeViewSelectionByIndex(0);		
				

				if (setFocus)
					TreeFolderBrowser.Focus();

			}, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
		}

		private void SetTreeViewSelectionByIndex(int index)
		{
			TreeViewItem item = (TreeViewItem)TreeFolderBrowser
				.ItemContainerGenerator
				.ContainerFromIndex(index);
			item.IsSelected = true;
		}

		private void SetTreeViewSelectionByItem(PathItem item)
		{
			TreeViewItem treeitem = GetTreeviewItem(item);				
			if (treeitem != null)
			{				
				treeitem.BringIntoView();

				if (treeitem.Parent != null && treeitem.Parent is TreeViewItem)
					((TreeViewItem)treeitem.Parent).IsExpanded = true;
				
				treeitem.IsSelected = true;		
			}
		}

		private TreeViewItem GetTreeviewItem(object item)
		{
			return  (TreeViewItem)TreeFolderBrowser
				.ItemContainerGenerator
				.ContainerFromItem(item);
		}

		private void TextFolderPath_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter || e.Key == Key.Tab)
			{
				TreeFolderBrowser.Focus();
			}
		}
		#endregion


		#region TreeView Selection Handling

		private void TreeView_Keyup(object sender, KeyEventArgs e)
		{
			var selected = TreeFolderBrowser.SelectedItem as PathItem;
			if (selected == null)
				return;

			if (e.Key == Key.Enter || e.Key == Key.Tab)
			{
				if (!selected.IsEditing)
					HandleSelection();
				else
					RenameFileOrFolder();
			}
			else if (e.Key == Key.Escape)
			{
				if (selected.IsEditing)
					selected.IsEditing = false;
			}
			else if (e.Key == Key.F2)
			{
				if (!selected.IsEditing)
					MenuRenameFile_Click(null, null);
			}
			else if (e.Key == Key.Delete)
			{
				if (!selected.IsEditing)
					MenuDeleteFile_Click(null, null);
			}
			else if (e.Key== Key.N)			
			{
				if (!selected.IsEditing)
				{
					MenuAddFile_Click(null, null);
					e.Handled = true;
				}
			}
		}

		void HandleSelection()
		{
			var fileItem = TreeFolderBrowser.SelectedItem as PathItem;
			if (fileItem == null || fileItem.IsFolder)
				return;

			OpenFile(fileItem.FullPath);			
		}

		void RenameFileOrFolder()
		{
			var fileItem = TreeFolderBrowser.SelectedItem as PathItem;
			if (fileItem == null)
				return;
			
			string oldFilename = System.IO.Path.GetFileName(fileItem.FullPath);
			string oldPath = System.IO.Path.GetDirectoryName(fileItem.FullPath);
			string newPath = System.IO.Path.Combine(oldPath, fileItem.EditName);

			if (newPath != fileItem.FullPath)
			{
				if (fileItem.IsFolder)
				{
					try
					{
						if (Directory.Exists(fileItem.FullPath))
							Directory.Move(fileItem.FullPath, newPath);
						else
							Directory.CreateDirectory(newPath);
					}
					catch (Exception ex)
					{
						MessageBox.Show("Unable to rename or create folder:\r\n" +
						                newPath, "Path Creation Error",
							MessageBoxButton.OK, MessageBoxImage.Warning);
					}
				}
				else
				{
					try
					{
						if (File.Exists(fileItem.FullPath))
							File.Move(fileItem.FullPath, newPath);
						else							
							File.WriteAllText(newPath,"");
					}
					catch (Exception ex)
					{
						MessageBox.Show("Unable to rename or create file:\r\n" +
						                newPath, "File Creation Error",
							MessageBoxButton.OK, MessageBoxImage.Warning);
					}

					HandleSelection();
				}
			}

			fileItem.FullPath = newPath;
			fileItem.IsEditing = false;
		}

		public void OpenFile(string file)
		{			
			string format = mmFileUtils.GetEditorSyntaxFromFileType(file);
			if (!string.IsNullOrEmpty(format))
			{
				mmApp.Model.Window.OpenTab(file);
				return;				
			}

			var ext = Path.GetExtension(file).ToLower().Replace(".","");
			if (StringUtils.Inlist(ext, "jpg", "png", "gif", "jpeg"))
			{
				if (!mmFileUtils.OpenImageInImageViewer(file))
				{
					MessageBox.Show("Unable to launch image viewer " + Path.GetFileName(mmApp.Configuration.ImageViewer) + 
						"\r\n\r\n" +
						"Most likely the image viewer configured in settings is not valid. Please check the 'ImageEditor' key in the Markdown Monster Settings." +
						"\r\n\r\n" +
						"We're opening the Settings file for you in the editor now.",
						"Image Launching Error",
						MessageBoxButton.OK, MessageBoxImage.Warning);

					mmApp.Model.Window.OpenTab(Path.Combine(mmApp.Configuration.CommonFolder,"MarkdownMonster.json"));
				}
			}		
			else
			{
				try
				{
					ShellUtils.GoUrl(file);
				}
				catch
				{
					mmApp.Model.Window.ShowStatus("Unable to open file " + file,4000);
					mmApp.Model.Window.SetStatusIcon(FontAwesome.WPF.FontAwesomeIcon.Warning, Colors.Red);

					if (MessageBox.Show("Unable to open this file. Do you want to open it as a text document in the editor?",
						    mmApp.ApplicationName,
						    MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
						mmApp.Model.Window.OpenTab(file);
				}
			}
		}

		private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
				HandleSelection();
		}

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#region Context Menu Actions

		private void MenuDeleteFile_Click(object sender, RoutedEventArgs e)
		{
			var selected = TreeFolderBrowser.SelectedItem as PathItem;
			if (selected == null)
				return;

			if (MessageBox.Show(
					"Delete:\r\n" +
					selected.FullPath + "\r\n\r\n" +
					"Are you sure?",
					"Delete File",
					MessageBoxButton.YesNo,
					MessageBoxImage.Question) != MessageBoxResult.Yes)
				return;
						

			try
			{
				FileInfo fi;

				//Directory.Delete(selected.FullPath, true);
				//File.Delete(selected.FullPath);

				// Recyle Bin Code can handle both files and directories
				mmFileUtils.MoveToRecycleBin(selected.FullPath);
				
				var parent = selected.Parent;

				var file = parent?.Files?.FirstOrDefault(fl => fl.FullPath == selected.FullPath);
				if (file != null)
					selected.Parent?.Files.Remove(file);

				// Delay required to overcome editor focus after MsgBox
				Dispatcher.Delay(700, (s) =>
				{
					TreeFolderBrowser.Focus();
					SetTreeViewSelectionByItem(parent);
					TreeFolderBrowser.Focus();
				});
			}
			catch (Exception ex)
			{
				mmApp.Model.Window.ShowStatus("Delete operation failed: " + ex.Message, 6000);
				mmApp.Model.Window.SetStatusIcon(FontAwesome.WPF.FontAwesomeIcon.Warning, Colors.Red);
			}
		}

		private void MenuAddFile_Click(object sender, RoutedEventArgs e)
		{
			var selected = TreeFolderBrowser.SelectedItem as PathItem;
			if (selected == null || selected.Parent == null)
				return;

			string path;
			if (!selected.IsFolder)
				path = Path.Combine(Path.GetDirectoryName(selected.FullPath), "NewFile.md");
			else
			{
				var treeItem = GetTreeviewItem(selected);
				if(treeItem!= null)
					treeItem.IsExpanded = true;

				path = Path.Combine(selected.FullPath, "NewFile.md");
			}

			var item = new PathItem
			{
				FullPath = path,
				IsFolder = false,
				IsEditing = true,
				IsSelected = true
			};

			if (!selected.IsFolder)
				item.Parent = selected.Parent;
			else
				item.Parent = selected;

			item.Parent.Files.Insert(0, item);
			SetTreeViewSelectionByItem(item);		
		}


		private void MenuAddDirectory_Click(object sender, RoutedEventArgs e)
		{
			var selected = TreeFolderBrowser.SelectedItem as PathItem;
			if (selected == null || selected.Parent == null)
				return;

			string path;
			if (!selected.IsFolder)
				path = Path.Combine(Path.GetDirectoryName(selected.FullPath), "NewFolder");
			else
			{
				var treeItem = GetTreeviewItem(selected);
				if (treeItem != null)
					treeItem.IsExpanded = true;

				path = Path.Combine(selected.FullPath, "NewFolder");
			}

			var item = new PathItem
			{
				FullPath = path,
				IsFolder = true,
				IsEditing = true,
				IsSelected = true
			};

			if (!selected.IsFolder)
				item.Parent = selected.Parent;
			else
				item.Parent = selected;

			item.Parent.Files.Insert(0, item);
		
			SetTreeViewSelectionByItem(item);
		}

		private void MenuRenameFile_Click(object sender, RoutedEventArgs e)
		{
			var selected = TreeFolderBrowser.SelectedItem as PathItem;
			if (selected == null)
				return;

			// Start Editing the file name
			selected.EditName = selected.DisplayName;
			selected.IsEditing = true;
		}

		#endregion


		#region Shell/Terminal Operations
		private void MenuOpenInExplorer_Click(object sender, RoutedEventArgs e)
		{
			string folder = FolderPath;

			var selected = TreeFolderBrowser.SelectedItem as PathItem;
			if (selected != null)
				folder = selected.FullPath; // Path.GetDirectoryName(selected.FullPath);

			if (string.IsNullOrEmpty(folder))
				return;

			if (selected == null || selected.IsFolder)
				ShellUtils.GoUrl(folder);
			else
				Process.Start("explorer.exe", "/select,\"" + folder + "\"");
		}

		private void MenuOpenTerminal_Click(object sender, RoutedEventArgs e)
		{
			string folder = FolderPath;

			var selected = TreeFolderBrowser.SelectedItem as PathItem;
			if (selected != null)
				folder = Path.GetDirectoryName(selected.FullPath);

			if (string.IsNullOrEmpty(folder))
				return;

			mmFileUtils.OpenTerminal(folder);
		}

		private void MenuOpenWithShell_Click(object sender, RoutedEventArgs e)
		{
			var selected = TreeFolderBrowser.SelectedItem as PathItem;
			if (selected == null)
				return;
			
			try
			{
				ShellUtils.GoUrl(selected.FullPath);
			}
			catch
			{
				mmApp.Model.Window.ShowStatus("Unable to open file " + selected.FullPath, 4000);
				mmApp.Model.Window.SetStatusIcon(FontAwesome.WPF.FontAwesomeIcon.Warning, Colors.Red);
			}
		}


		private void MenuOpenInEditor_Click(object sender, RoutedEventArgs e)
		{
			var selected = TreeFolderBrowser.SelectedItem as PathItem;
			if (selected == null)
				return;

			if (selected.IsFolder)
				ShellUtils.GoUrl(selected.FullPath);
			else
				mmApp.Model.Window.OpenTab(selected.FullPath);
		}

		private void MenuShowImage_Click(object sender, RoutedEventArgs e)
		{
			var selected = TreeFolderBrowser.SelectedItem as PathItem;
			if (selected == null)
				return;

			mmFileUtils.OpenImageInImageViewer(selected.FullPath);
		}

		private void MenuEditImage_Click(object sender, RoutedEventArgs e)
		{
			var selected = TreeFolderBrowser.SelectedItem as PathItem;
			if (selected == null)
				return;

			mmFileUtils.OpenImageInImageEditor(selected.FullPath);
		}

		#endregion

		private string overImage;

		private void TextFileOrFolderName_MouseEnter(object sender, MouseEventArgs e)
		{
			dynamic s = sender as dynamic;

			var selected = s.DataContext as PathItem;
			if (selected == null)
				return;

			overImage = selected.FullPath;

			Dispatcher.Delay(600, imageFile =>
			{				
				if (string.IsNullOrEmpty(overImage) || overImage != (string) imageFile)
					return;

				try
				{
					ImagePreview.Source = new BitmapImage(new Uri((string) imageFile));
					PopupImagePreview.IsOpen = true;
				}
				catch
				{ }
			}, overImage);

		}

		private void TextFileOrFolderName_MouseLeave(object sender, MouseEventArgs e)
		{
			overImage = null;

			PopupImagePreview.IsOpen = false;
			ImagePreview.Source = null;
			ImagePreviewColumn.Height = new GridLength(0);

		}

		#region Drag Operations

		private System.Windows.Point startPoint;

		private void TreeFolderBrowser_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			startPoint = e.GetPosition(null);
		}

		private void TreeFolderBrowser_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				var selected = TreeFolderBrowser.SelectedItem as PathItem;

				// only drag image files
				if (selected == null || !selected.IsImage)
					return;

				var mousePos = e.GetPosition(null);
				var diff = startPoint - mousePos;

				if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance
				    || Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
				{
					var treeView = sender as TreeView;
					var treeViewItem = FindCommonVisualAncestor((DependencyObject) e.OriginalSource);
					if (treeView == null || treeViewItem == null)
						return;
					
					var dragData = new DataObject(DataFormats.UnicodeText, selected.FullPath);
					DragDrop.DoDragDrop(treeViewItem, dragData, DragDropEffects.Copy);
				}
			}
		}

		#endregion
	}
}
