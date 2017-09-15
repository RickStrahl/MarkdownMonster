using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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


	    /// <summary>
	    /// Determines whether icons are displayed in folder browser        
	    /// </summary>
	    public bool ShowIcons { get; set; } = true;

	    



		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
