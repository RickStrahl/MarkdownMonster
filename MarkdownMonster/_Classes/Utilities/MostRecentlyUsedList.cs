using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownMonster
{
	public static class MostRecentlyUsedList
	{
		/// <summary>
		/// Adds Recently Used Document to the MRU list in Windows.
		/// Item is added to the global MRU list as well as to the
		/// application specific shortcut that is associated with
		/// the application and shows up in the task bar icon MRU list.
		/// </summary>
		/// <param name="path">Full path of the file</param>
		public static void AddToRecentlyUsedDocs(string path)
		{
			SHAddToRecentDocs(ShellAddToRecentDocsFlags.Path, path);
		}


		private enum ShellAddToRecentDocsFlags
		{
			Pidl = 0x001,
			Path = 0x002,
		}

		[DllImport("shell32.dll", CharSet = CharSet.Ansi)]
		private static extern void
			SHAddToRecentDocs(ShellAddToRecentDocsFlags flag, string path);

	}

}
