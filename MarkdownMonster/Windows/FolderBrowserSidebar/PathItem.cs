using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using MarkdownMonster.Annotations;
using Westwind.Utilities;

namespace MarkdownMonster.Windows
{
	/// <summary>
	/// Represents a file or folder in the Folder Browser side panel control.
	/// </summary>
	[DebuggerDisplay("Path: {FullPath}")]
	public class PathItem : INotifyPropertyChanged
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

		public string FullPath
		{
			get => _fullPath;
		    set
			{
				if (value == _fullPath) return;
				_fullPath = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(DisplayName));
				OnPropertyChanged(nameof(IsImage));
				OnPropertyChanged(nameof(IsNotImage));
			    OnPropertyChanged(nameof(Icon));
			}
		}
		private string _fullPath;


		public PathItem Parent { get; set; }

		public bool IsFolder
		{
			get => _isFolder;
		    set
			{
				if (value == _isFolder) return;
				_isFolder = value;
                OnPropertyChanged();
				OnPropertyChanged(nameof(DisplayName));
			}
		}
		private bool _isFolder;


	    public bool IsFile
	    {
	        get => _isFile;
	        set
	        {
	            if (value == _isFile) return;
	            _isFile = value;
	            OnPropertyChanged(nameof(IsFile));
	        }
	    }
	    private bool _isFile;


	    
	    public bool IsVisible
	    {
	        get { return _isVisible; }
	        set
	        {
	            if (value == _isVisible) return;
	            _isVisible = value;
	            OnPropertyChanged();
	        }
	    }

	    private bool _isVisible = true;


        public bool IsExpanded
	    {
	        get { return _isExpanded; }
	        set
	        {
	            if (value == _isExpanded) return;
	            _isExpanded = value;
	            OnPropertyChanged(nameof(IsExpanded));
	        }
	    }
	    private bool _isExpanded;

	    public string EditName
		{
			get => _editName;
            set
			{
				if (value == _editName) return;
				_editName = value;

				OnPropertyChanged();
				OnPropertyChanged(nameof(DisplayName));
			}
		}
		private string _editName;


		public bool IsEditing
		{
			get => _isEditing;
		    set
			{
				if (value == _isEditing) return;
				_isEditing = value;
				OnPropertyChanged(nameof(IsEditing));
				OnPropertyChanged(nameof(IsNotEditing));
			}
		}
		private bool _isEditing = false;

		public bool IsNotEditing => !_isEditing;


	    public bool IsSelected
		{
			get => _isSelected;
	        set
			{
				if (value == _isSelected) return;
				_isSelected = value;
				OnPropertyChanged(nameof(IsSelected));
			    OnPropertyChanged(nameof(IsImage));
            }
		}
		private bool _isSelected;



		public bool IsImage
		{

			get
			{
				if (ImageUtils.GetImageMediaTypeFromFilename(FullPath) == "application/image")
					return false;
				return true;
			}
		}

		public bool IsNotImage => !IsImage;


		public ObservableCollection<PathItem> Files
		{
			get
			{
				if (_files == null)
					_files = new ObservableCollection<PathItem>();
				return _files;
			}
			set => _files = value;
		}



	    public ImageSource Icon
	    {
	        get => _icon;
	        set
	        {
	            if (Equals(value, _icon)) return;
	            _icon = value;
	            OnPropertyChanged(nameof(Icon));
	        }
	    }
	    private ImageSource _icon;

	    public ImageSource OpenIcon
	    {
	        get
	        {
                if (_openicon == null)
                    return Icon;

	            return _openicon;
	        }
	        set
	        {
	            if (Equals(value, _openicon)) return;
	            _openicon = value;
	            OnPropertyChanged(nameof(OpenIcon));
	        }
	    }

	    public static PathItem Empty { get; } = new PathItem();

	    private ImageSource _openicon;

        private ObservableCollection<PathItem> _files;

		public override string ToString()
		{
			return this.DisplayName;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

	    public void SetIcon(string explicitIconName = null)
	    {
	        if (!string.IsNullOrEmpty(explicitIconName))
	            Icon = FolderStructure.IconList.GetIconFromFile(explicitIconName);
            if (!IsFolder)
	            Icon = FolderStructure.IconList.GetIconFromFile(FullPath);
            else
            {
                Icon = FolderStructure.IconList.GetIconFromFile("folder.folder"); // special case
                OpenIcon = FolderStructure.IconList.GetIconFromFile("folder.openfolder"); // special case
            }
        }	
	}
}
