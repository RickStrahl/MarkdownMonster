using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.IO.Path;

namespace MarkdownMonster.Windows
{
	public class FolderStructure
	{

		/// <summary>
		/// Gets a folder hierarchy
		/// </summary>
		/// <param name="baseFolder"></param>
		/// <param name="parentPathItem"></param>
		/// <param name="skipFolders"></param>
		/// <returns></returns>
		public PathItem GetFilesAndFolders(string baseFolder, PathItem parentPathItem = null, string skipFolders = "node_modules,.git")
		{
			if (string.IsNullOrEmpty(baseFolder) || !Directory.Exists(baseFolder))
				return new PathItem();

			PathItem activeItem;

			if (parentPathItem == null)
			{
				activeItem = new PathItem
				{					
					FullPath = baseFolder,
					IsFolder = true
				};
				parentPathItem = activeItem;				
			}
			else
			{
				activeItem = new PathItem { FullPath=baseFolder, IsFolder = true, Parent = parentPathItem};
				parentPathItem.Files.Add(activeItem);				
			}

			

			var folders = Directory.GetDirectories(baseFolder);
			foreach (var folder in folders)
			{
				GetFilesAndFolders(folder, activeItem, skipFolders);
			}
			var files = Directory.GetFiles(baseFolder);
			foreach (var file in files)
			{
				activeItem.Files.Add(new PathItem { FullPath=file, Parent = parentPathItem, IsFolder=false });
			}

			return activeItem;
		}

		
	}

	public class PathItem
	{
		public string DisplayName
		{
			get
			{
				string path = Path.GetFileName(FullPath);
				if (IsFolder && string.IsNullOrEmpty(path))
					path = "root";
				return path;
			}
		}


		public string FullPath { get; set; }
		
		public PathItem Parent { get; set; }

		public bool IsFolder { get; set; }

		public ObservableCollection<PathItem> Files
		{
			get
			{
				if (_files == null)
					_files = new ObservableCollection<PathItem>();
				return _files;
			}
			set { _files = value; }
		}
		private ObservableCollection<PathItem> _files;

		public override string ToString()
		{
			return this.DisplayName;
		}
	}
}
