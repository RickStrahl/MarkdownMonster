using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using MarkdownMonster.Annotations;

namespace MarkdownMonster.Configuration
{
	public class FolderBrowserConfiguration : INotifyPropertyChanged
	{

		/// <summary>
		/// The width of the File browser
		/// </summary>
		public int WindowWidth
		{
			get { return _WindowWidth; }
			set
			{
				if (value == _WindowWidth) return;
				_WindowWidth = value;
				OnPropertyChanged(nameof(WindowWidth));
			}
		}
		private int _WindowWidth = 300;


		/// <summary>
		/// Determines whether the File Browser window is visible
		/// </summary>
		public bool Visible
		{
			get { return _Visible; }
			set
			{
				if (value == _Visible) return;
				_Visible = value;
				OnPropertyChanged(nameof(Visible));
			}
		}
		private bool _Visible = false;


		/// <summary>
		/// The last file that was active
		/// </summary>
		public string FolderPath
		{
			get { return _folderPath; }
			set
			{
				if (value == _folderPath) return;
				_folderPath = value;
				OnPropertyChanged(nameof(FolderPath));			    
			}
		}
	    private string _folderPath;


	    public List<string> RecentFolders { get; set; } = new List<string>();

	    /// <summary>
	    /// Determines whether icons are displayed in folder browser        
	    /// </summary>
	    public bool ShowIcons { get; set; } = true;


	    public void AddRecentFolder(string folder)
	    {
            if (string.IsNullOrEmpty(folder))
                return;

	        var existing = RecentFolders.FirstOrDefault(f => f.ToLower().Contains(folder.ToLower()));
	        if (existing != null)
	            RecentFolders.Remove(existing);

	        RecentFolders.Insert(0, folder);
            RecentFolders = RecentFolders.Take(mmApp.Configuration.RecentDocumentsLength).ToList();
	    }

	    public void RecentFolderContextMenu(ContextMenu contextMenu)
	    {
	        contextMenu.Items.Clear();

	        foreach (var folder in RecentFolders)
	        {
	            var mi = new MenuItem()
	            {
	                Header = folder
	            };
                mi.Click += Mi_Click;
	            contextMenu.Items.Add(mi);
	        }


	    }

        private void Mi_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null)
                return;

            var folder = menuItem.Header as string;
            mmApp.Model.Window.FolderBrowser.FolderPath = folder;
        }

        public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
