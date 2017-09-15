using System.Collections.ObjectModel;
using System.ComponentModel;
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
			get { return _fullPath; }
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
			get { return _isFolder; }
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
	        get { return _isFile; }
	        set
	        {
	            if (value == _isFile) return;
	            _isFile = value;
	            OnPropertyChanged();
	        }
	    }
	    private bool _isFile;


        public string EditName
		{
			get { return _editName; }
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
			get { return _isEditing; }
			set
			{
				if (value == _isEditing) return;
				_isEditing = value;
				OnPropertyChanged(nameof(IsEditing));
				OnPropertyChanged(nameof(IsNotEditing));
			}
		}
		private bool _isEditing = false;

		public bool IsNotEditing
		{
			get { return !_isEditing; }			
		}


		public bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (value == _isSelected) return;
				_isSelected = value;
				OnPropertyChanged(nameof(IsSelected));
			}
		}
		private bool _isSelected;

		

		public bool IsImage
		{

			get
			{
				if (mmFileUtils.GetImageMediaTypeFromFilename(FullPath) == "application/image")
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
			set { _files = value; }
		}

	    

	    public ImageSource Icon
	    {
	        get { return _icon; }
	        set
	        {
	            if (Equals(value, _icon)) return;
	            _icon = value;
	            OnPropertyChanged(nameof(Icon));
	        }
	    }
	    private ImageSource _icon;


        private ObservableCollection<PathItem> _files;

		public override string ToString()
		{
			return this.DisplayName;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}