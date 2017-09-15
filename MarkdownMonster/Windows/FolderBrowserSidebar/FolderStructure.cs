using System.IO;
using MarkdownMonster.Utilities;


namespace MarkdownMonster.Windows
{
	public class FolderStructure
	{
	    private AssociatedIcons icons = new AssociatedIcons();

		/// <summary>
		/// Gets a folder hierarchy
		/// </summary>
		/// <param name="baseFolder"></param>
		/// <param name="parentPathItem"></param>
		/// <param name="skipFolders"></param>
		/// <returns></returns>
		public PathItem GetFilesAndFolders(string baseFolder, PathItem parentPathItem = null, string skipFolders = "node_modules,bower_components,packages,testresults,bin,obj")
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


			string[] folders = null;
			
			try
			{
				folders = Directory.GetDirectories(baseFolder);
			}
			catch { }

			if (folders != null)
			{
				foreach (var folder in folders)
				{
					var name = System.IO.Path.GetFileName(folder);
					if (!string.IsNullOrEmpty(name))
					{
						if (name.StartsWith("."))
							continue;
						// skip folders
						if (("," + skipFolders + ",").Contains("," + name.ToLower() + ","))
							continue;
					}


					GetFilesAndFolders(folder, activeItem, skipFolders);
				}			
			}

			string[] files = null;
			try
			{
				files = Directory.GetFiles(baseFolder);
			}
			catch { }

			if (files != null)
			{
				foreach (var file in files)
				{

				    var item = new PathItem {FullPath = file, Parent = activeItem, IsFolder = false, IsFile = true};
				    if (mmApp.Configuration.FolderBrowser.ShowIcons)
				        item.Icon = icons.GetIconFromFile(file);


				    activeItem.Files.Add(item);
				}
			}

			return activeItem;
		}

		
	}
}
