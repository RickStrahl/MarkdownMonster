using System;
using System.IO;
using System.Linq;
using System.Windows.Media;
using FontAwesome.WPF;
using LibGit2Sharp;
using MarkdownMonster.Utilities;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
    public class FolderStructure
	{
	    internal static AssociatedIcons IconList = new AssociatedIcons();

        
		/// <summary>
		/// Gets a folder hierarchy and attach to an existing folder item or 
		/// </summary>
		/// <param name="baseFolder">The folder for which to retrieve files for</param>
		/// <param name="parentPathItem">The parent item to which the retrieved files are attached</param>
		/// <param name="ignoredFolders"></param>
		/// <returns></returns>
		public PathItem GetFilesAndFolders(string baseFolder,
		            PathItem parentPathItem = null,
		            string ignoredFolders = null,
		            string ignoredFileExtensions = null,
		            bool nonRecursive = false)
		{
			if (string.IsNullOrEmpty(baseFolder) || !Directory.Exists(baseFolder) )
				return new PathItem();

		    baseFolder = ExpandPathEnvironmentVars(baseFolder);

			PathItem activeItem;
            bool isRootFolder = false;

		    if (ignoredFolders == null)
		        ignoredFolders = mmApp.Configuration.FolderBrowser.IgnoredFolders;
            if(ignoredFileExtensions == null)
                ignoredFileExtensions = mmApp.Configuration.FolderBrowser.IgnoredFileExtensions;

            if (parentPathItem == null)
			{
                isRootFolder = true;
                activeItem = new PathItem
                {
                    FullPath = baseFolder,
                    IsFolder = true
                };
			    if (mmApp.Configuration.FolderBrowser.ShowIcons)
			    {
			        activeItem.SetIcon();			        
			    }                
			}
			else
			{
			    activeItem = parentPathItem; //new PathItem { FullPath=baseFolder, IsFolder = true, Parent = parentPathItem};
                activeItem.Files.Clear(); // remove all items
            }

            string[] folders = null;

			try
			{
				folders = Directory.GetDirectories(baseFolder);			   
			}
			catch { }

			if (folders != null)
			{
                foreach (var folder in folders.OrderBy(f=> f.ToLower()))
				{
					var name = Path.GetFileName(folder);
					if (!string.IsNullOrEmpty(name))
					{
						if (name.StartsWith("."))
							continue;
						// skip folders
						if (("," + ignoredFolders + ",").Contains("," + name.ToLower() + ","))
							continue;
					}

				    var folderPath = new PathItem
				    {
				        FullPath = folder,
				        IsFolder = true,
				        Parent = activeItem
				    };
				    if (mmApp.Configuration.FolderBrowser.ShowIcons)
				        folderPath.SetIcon();
				    activeItem.Files.Add(folderPath);

                    if (!nonRecursive)				        
                        GetFilesAndFolders(folder, folderPath, ignoredFolders);
                    else
				    {
				        folderPath.Files.Add(PathItem.Empty);
				    }
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
                // Skip Extensions
                string[] extensions = null;
			    if (!string.IsNullOrEmpty(ignoredFileExtensions))			    
			        extensions = ignoredFileExtensions.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
			                        
				foreach (var file in files.OrderBy(f=> f.ToLower()))
				{
                    if (extensions != null &&
                        extensions.Any(ext => file.EndsWith(ext,StringComparison.InvariantCultureIgnoreCase)))
                        continue;
                    
                    var item = new PathItem {FullPath = file, Parent = activeItem, IsFolder = false, IsFile = true};
				    if (mmApp.Configuration.FolderBrowser.ShowIcons)
				        item.Icon = IconList.GetIconFromFile(file);
                    
				    activeItem.Files.Add(item);
				}
			}

		    if (activeItem.FullPath.Length > 5 && isRootFolder )
		    {
		        var parentFolder = new PathItem
		        {
		            IsFolder = true,
		            FullPath = ".."
		        };
		        parentFolder.SetIcon();
		        activeItem.Files.Insert(0, parentFolder);
		    }
            
		    return activeItem;
		}



        /// <summary>
        /// Updates a path item folder structure with Git Status information
        /// </summary>
        /// <param name="rootFolder"></param>
        /// <param name="repo"></param>
	    public void UpdateGitFileStatus(PathItem rootFolder, Repository repo = null)
	    {
	        bool repoCreated = false;
	        if (repo == null)
	        {
	            var helper = new GitHelper();
                repo = helper.OpenRepository(rootFolder.FullPath);
                if (repo == null)
                    return;  // no repo

	            repoCreated = true;
            }

            foreach (var pi in rootFolder.Files)
	        {
                if (pi?.FullPath == null  || pi.FullPath == "..")
                    continue;

	            if (pi.IsFolder)
	            {
	                UpdateGitFileStatus(pi, repo);
	                continue;
	            }

	            var status = repo.RetrieveStatus(pi.FullPath);
	            if (pi.FileStatus != status)
                    pi.FileStatus = status;
	        }

	        if (repoCreated)
	            repo.Dispose();
	    }






        /// <summary>
        /// Searches the tree for a specific item
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="fullName"></param>
        /// <returns></returns>
	    public PathItem FindPathItemByFilename(PathItem parent, string fullName)
        {
            // check for root folder match
            if (parent.FullPath.Equals(fullName, StringComparison.InvariantCultureIgnoreCase))
                return parent;

            // Files first for perf
            foreach (var item in parent.Files.Where(pi => pi.IsFile))
            {
                if (string.IsNullOrEmpty(item.FullPath)) continue; // prevent placeholder errors

                if (item.FullPath.Equals(fullName, StringComparison.InvariantCultureIgnoreCase))
                    return item;
            }

            // then directories recursively
            foreach (var item in parent.Files.Where(pi => !pi.IsFile))
            {
                if (item.FullPath.Equals(fullName,StringComparison.InvariantCultureIgnoreCase))
                    return item;

                if (item.IsFolder && item.Files != null && item.Files.Count > 0)
	            {
	                var childItem = FindPathItemByFilename(item, fullName);
                    if (childItem != null)
                        return childItem;
	            } 
	        }

            return null;
	    }

        /// <summary>
        /// Inserts a path item in a parent in the proper alphabetical order
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="parent"></param>
	    public void InsertPathItemInOrder(PathItem pi, PathItem parent)
	    {            
            int foundIndex = -1;
	        foreach (var pitem in parent.Files)
	        {
                if (pitem?.FullPath == null)
                    continue;

	            foundIndex++;
	            if (pitem.IsFolder && !pi.IsFolder || !pitem.IsFolder && pi.IsFolder)
	                continue;
               
	            if (string.Compare(pi.FullPath,pitem.FullPath,StringComparison.InvariantCultureIgnoreCase) < 0)
	                break;
	        }

	        if (foundIndex < 0) foundIndex = 0;

            mmApp.Model.Window.Dispatcher.Invoke(() =>
            {
                parent.Files.Insert(foundIndex, pi);
                pi.UpdateGitFileStatus();
            });
        }

        /// <summary>
        /// Sets visibility of all items in the path item tree
        /// </summary>
        /// <param name="searchText"></param>
        /// <param name="pathItem"></param>
	    public void SetSearchVisibility(string searchText, PathItem pathItem, bool recursive)
        {
            if (searchText == null)
                searchText = string.Empty;
	        searchText = searchText.ToLower();
            
            // no items below
	        if (pathItem.Files.Count == 1 && pathItem.Files[0] == PathItem.Empty)
	        {
                if (!recursive)
	                return;

                // load items
	            var files = GetFilesAndFolders(pathItem.FullPath, pathItem, nonRecursive: !recursive).Files;
	            pathItem.Files.Clear();
	            foreach (var file in files)
	                pathItem.Files.Add(file);

                // required so change is detected by tree
	            //pathItem.OnPropertyChanged(nameof(PathItem.Files));
	        }

	        foreach (var pi in pathItem.Files)
	        {                
	            if (string.IsNullOrEmpty(searchText) || pi.FullPath == "..")	            
	            {
	                pi.IsVisible = true;
                    pi.IsExpanded = false;
	            }
                else if (pi.DisplayName.ToLower().Contains(searchText))
	            {
	                pi.IsVisible = true;
	                var parent = pi.Parent;

	                while (parent != null)
	                {
	                    parent.IsExpanded = true;
	                    parent.OnPropertyChanged(nameof(PathItem.IsExpanded));
	                    parent.IsVisible = true;

                        parent = parent.Parent;
	                }
                }
	            else
                    pi.IsVisible = false;

	            if (pi.IsFolder && recursive)
	                SetSearchVisibility(searchText, pi, recursive);
	        }
            
	    }


	    public static string ExpandPathEnvironmentVars(string path)
	    {
	        string result = path;
	        while (path.Contains("%"))
	        {
	            var extract = StringUtils.ExtractString(result, "%", "%");
	            if (string.IsNullOrEmpty(extract))
	                return result;

	            var env = Environment.GetEnvironmentVariable(extract);
	            if (!string.IsNullOrEmpty(env))
	                result = result.Replace("%" + extract + "%", env);
	            else
	                return result;
	        }

	        return result;
	    }
    }

    public class SourceControlIcons
    {
        public static ImageSource Normal;
        public static ImageSource Changed;
        public static ImageSource Ignored;
        public static ImageSource Added;
        public static ImageSource Conflict;
        public static ImageSource Unversioned;
        public static ImageSource Deleted;

        static SourceControlIcons()
        {
            Normal = ImageAwesome.CreateImageSource(FontAwesomeIcon.Lock, Brushes.SteelBlue);
            Changed = ImageAwesome.CreateImageSource(FontAwesomeIcon.Check, Brushes.Red);
            Ignored = ImageAwesome.CreateImageSource(FontAwesomeIcon.MinusCircle, Brushes.LightSlateGray);
            Added = ImageAwesome.CreateImageSource(FontAwesomeIcon.Plus, Brushes.LightGreen);
            Conflict = ImageAwesome.CreateImageSource(FontAwesomeIcon.Warning, Brushes.DarkGoldenrod);
            Unversioned = ImageAwesome.CreateImageSource(FontAwesomeIcon.MinusCircle, Brushes.LightSlateGray);
            Deleted = ImageAwesome.CreateImageSource(FontAwesomeIcon.MinusCircle, Brushes.IndianRed);
        }
    }
}

