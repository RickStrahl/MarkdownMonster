using System.IO;
using MarkdownMonster.Utilities;


namespace MarkdownMonster.Windows
{
	public class FolderStructure
	{
	    internal static AssociatedIcons icons = new AssociatedIcons();

		/// <summary>
		/// Gets a folder hierarchy
		/// </summary>
		/// <param name="baseFolder"></param>
		/// <param name="parentPathItem"></param>
		/// <param name="skipFolders"></param>
		/// <returns></returns>
		public PathItem GetFilesAndFolders(string baseFolder, PathItem parentPathItem = null, string skipFolders = "node_modules,bower_components,packages,testresults,bin,obj", bool nonRecursive = false)
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
			    if (mmApp.Configuration.FolderBrowser.ShowIcons)
			    {
			        activeItem.SetFolderIcon();
			        
			    }

			    parentPathItem = activeItem;
			}
			else
			{
				activeItem = new PathItem { FullPath=baseFolder, IsFolder = true, Parent = parentPathItem};
			    if (mmApp.Configuration.FolderBrowser.ShowIcons)
			        activeItem.SetFolderIcon();

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

				    if (!nonRecursive)
				        GetFilesAndFolders(folder, activeItem, skipFolders);
				    else
				    {
				        var folderPath = new PathItem
				        {
				            FullPath = folder,
				            IsFolder = true,
				            Parent = activeItem
				        };
				        if (mmApp.Configuration.FolderBrowser.ShowIcons)
				            folderPath.SetFolderIcon();

                        activeItem.Files.Add(folderPath);
				    }
				}
			}

			string[] files = null;
			try
			{
				files = Directory.GetFiles(baseFolder);
			}
			catch { }

		    if (folders == null && nonRecursive)
		    {
		        foreach (var folder in folders)
		        {

		        }
		    }
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
