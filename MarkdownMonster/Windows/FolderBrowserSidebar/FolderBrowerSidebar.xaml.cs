using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MarkdownMonster.Annotations;

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
					ActivePathItem = FolderStructure.GetFilesAndFolders(value);

				_folderPath = value;

				OnPropertyChanged(nameof(FolderPath));
				OnPropertyChanged(nameof(ActivePathItem));
			}
		}
		private string _folderPath;
		

		//public string FolderPath
		//{
		//	get { return (string)GetValue(FolderPathProperty); }
		//	set
		//	{
		//		if (!Directory.Exists(value))
		//			return;


		//		if (string.IsNullOrEmpty(value))
		//			ActivePathItem = new PathItem();
		//		else if (value != (string) GetValue(FolderPathProperty))				
		//			ActivePathItem = FolderStructure.GetFilesAndFolders(value);

		//		OnPropertyChanged();
		//		OnPropertyChanged(nameof(ActivePathItem));

		//		SetValue(FolderPathProperty, value);
		//	}
		//}

		//// Using a DependencyProperty as the backing store for FolderPath.  This enables animation, styling, binding, etc...
		//public static readonly DependencyProperty FolderPathProperty =
		//	DependencyProperty.Register("FolderPath", typeof(string), typeof(FolderBrowerSidebar), new PropertyMetadata(null));


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

		public FolderBrowerSidebar()
		{
			InitializeComponent();
			Loaded += FolderBrowerSidebar_Loaded;
		}
		

		private void FolderBrowerSidebar_Loaded(object sender, RoutedEventArgs e)
		{
			DataContext = this;

			//var OutputDriverDPD = DependencyPropertyDescriptor.FromProperty(OutputDriverProperty, typeof(MappingSelector));
			//OutputDriverDPD.AddValueChanged(this, (sender, args) =>
			//{
			//	OutputDriver = (Type)GetValue(OutputDriverProperty);
			//});

			
		}

		#region Folder Management


		#endregion

		private void ButtonUseCurrentFolder_Click(object sender, RoutedEventArgs e)
		{
			var doc = mmApp.Model.ActiveDocument;
			if (doc != null)
				FolderPath = System.IO.Path.GetDirectoryName(doc.Filename);
		}


		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#region TreeView Selection Handling

		void HandleSelection()
		{
			var fileItem = TreeFolderBrowser.SelectedItem as PathItem;
			if (fileItem == null || fileItem.IsFolder)
				return;

			mmApp.Model.Window.OpenTab(fileItem.FullPath);

			Dispatcher.DelayWithPriority(300, (s) => { TreeFolderBrowser.Focus(); });
		}

		private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
				HandleSelection();
		}

		private void TreeViewItem_Keyup(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
				HandleSelection();
		}
		#endregion
	}
}
