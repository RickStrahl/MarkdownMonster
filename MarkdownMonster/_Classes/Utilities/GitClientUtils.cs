using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownMonster
{
    public static class GitClientUtils
    {
        /// <summary>
        /// Opens the configured Git Client in the specified folder
        /// </summary>
        /// <param name="folder"></param>
        public static bool OpenGitClient(string folder)
        {

            var exe = mmApp.Configuration.Git.GitClientExecutable;
            if (string.IsNullOrEmpty(exe) || !File.Exists(exe))
                return false;

            //folder = GitHelper.FindGitRepositoryRoot(folder);

            try
            {
                var pi = Process.Start(exe, $"\"{folder}\"");

                if (pi == null)
                    return false;
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Tries to find GitHub Desktop in this computer.
        /// Returns null if not found
        /// </summary>
        /// <returns>Null if not found</returns>
        public static string FindGitClient_GitHubDesktop()
        {
            // Find installation in registry
            using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\GitHubDesktop"))
            {
                if (key != null)
                {
                    string gitLocation = key.GetValue("InstallLocation", null).ToString();
                    if (gitLocation != null)
                    {
                        string gitClientFilename = Path.Combine(gitLocation, "GitHubDesktop.exe");
                        if (File.Exists(gitClientFilename))
                            return gitClientFilename;
                    }
                }
            }

            // Find installation in folder
            string git = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GitHubDesktop\\GitHubDesktop.exe");
            if (File.Exists(git))
                return git;

            // Not found
            return null;
        }

        /// <summary>
        /// Tries to find a Git Client installed in this computer.
        /// Returns null if none found
        /// </summary>
        /// <returns>Null if Git Client is not found in this computer</returns>
        public static string FindGitClient()
        {
            string git = null;

            // SmartGit
            git = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "SmartGit\\bin\\SmartGit.exe");
            if (File.Exists(git))
                return git;

            // GitHub Desktop
            git = FindGitClient_GitHubDesktop();
            if (git != null) return git;

            // SourceTree
            git = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SourceTree\\sourcetree.exe");
            if (File.Exists(git))
                return git;

            // GitKraken
            git = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "gitkraken");
            if (Directory.Exists(git))
            {
                var di = new DirectoryInfo(git);

                di = di.GetDirectories("app-*", SearchOption.TopDirectoryOnly)
                    .OrderByDescending(d => d.LastWriteTime)
                    .FirstOrDefault(d => d.Name.StartsWith("app-"));

                if (di != null)
                {
                    return Path.Combine(di.FullName, "gitkraken.exe");
                }

            }

            // Not found
            return null;
        }


        /// <summary>
        /// Tries to find a Git Diff tool installed in this computer.
        /// Returns null if none found
        /// </summary>
        /// <returns>Null if Git Diff tool is not found in this computer</returns>
        public static string FindGitDiffTool()
        {
            string diff = null;

            diff = Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432"),
                "Beyond Compare 4\\BCompare.exe");
            if (File.Exists(diff))
                return diff;

            diff = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Meld\\Meld.exe");
            if (File.Exists(diff))
                return diff;

            diff = Path.Combine(Environment.GetEnvironmentVariable("ProgramW6432"),
                "KDiff\\KDiff.exe");
            if (File.Exists(diff))
                return diff;

            // Not found
            return null;
        }
    }
}
