using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using MarkdownMonster.Annotations;
using Microsoft.Alm.Authentication;
using Westwind.Utilities;
using CompareOptions = LibGit2Sharp.CompareOptions;

namespace MarkdownMonster.Utilities
{
    /// <summary>
    /// Helps with a number of Git Operations
    /// </summary>
    public class GitHelper : IDisposable
    {
        public Repository Repository { get; set; }

        public Func<string, bool> CloneProgress { get; set; }

        /// <summary>
        /// Opens a repository and stores it in the Repository
        /// property of this class
        /// </summary>
        /// <param name="localPath"></param>
        /// <returns></returns>
        public Repository OpenRepository(string localPath)
        {
            var repoPath = FindGitRepositoryRoot(localPath);
            if (repoPath == null)
            {
                SetError("No repository at: " + localPath);
                return null;
            }

            try
            {
                Repository = new Repository(repoPath);
                return Repository;
            }
            catch (Exception ex)
            {
                SetError(ex);
            }

            return null;
        }

        #region File Operations
        /// <summary>
        /// Returns the Git file status for an individual file
        /// </summary>
        /// <param name="file">Path to a file</param>
        /// <returns></returns>
        public FileStatus GetGitStatusForFile(string file)
        {
            if (!File.Exists(file))
                return FileStatus.Nonexistent;

            var path = Path.GetDirectoryName(file);
            using (var repo = OpenRepository(path))
            {
                if (repo == null)
                    return FileStatus.Nonexistent;

                return repo.RetrieveStatus(file);
            }
        }


        /// <summary>
        /// Removes any changes since the last commit on the current active local branch
        /// </summary>
        /// <param name="file"></param>
        public void UndoChanges(string file)
        {
            if (!File.Exists(file))
                return;

            var path = Path.GetDirectoryName(file);
            using (var repo = OpenRepository(path))
            {
                var branch = repo.Head.FriendlyName;
                //var relFile = FileUtils.GetRelativePath(file, repo.Info.WorkingDirectory);
                repo.CheckoutPaths(branch, new[] { file }, new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
            }
        }

        #endregion

        /// <summary>
        /// Clones a repository
        /// </summary>
        /// <param name="gitUrl"></param>
        /// <param name="localPath"></param>
        /// <param name="useGitCredentialManager"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool CloneRepository(string gitUrl,
                string localPath,
                bool useGitCredentialManager = false,
                string username = null,
                string password = null,
                string branch = "master"
            )
        {
            try
            {
                var options = new CloneOptions
                {
                    Checkout = true,
                    BranchName = branch
                };


                if (CloneProgress != null)
                    options.OnProgress = new LibGit2Sharp.Handlers.ProgressHandler(CloneProgress);

                if (useGitCredentialManager)
                {

                    var creds = GetGitCredentials(gitUrl);

                    if (creds?.Username != null)
                    {
                        options.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials()
                        {
                            Username = creds.Username,
                            Password = creds.Password
                        };
                    }
                    else
                    {
                        // oAuth flow then set credentials


                    }
                }
                else if (!string.IsNullOrEmpty(username))
                {

                    options.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials()
                    {
                        Username = username,
                        Password = password
                    };
                }

                Repository.Clone(gitUrl, localPath, options);
            }
            catch (Exception ex)
            {
                SetError(ex);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Clones a repository using the Command Line Tooling.
        /// Provides built-in authentication UI.
        /// </summary>
        /// <param name="gitUrl"></param>
        /// <param name="localPath"></param>
        /// <param name="depth">Depth of the Git revision history (shallow copy)</param>
        /// <returns></returns>
        public GitCommandResult CloneRepositoryCommandLine(string gitUrl, string localPath,
            Action<object, DataReceivedEventArgs> progress = null, int depth = 0)
        {
            try
            {
                string cloneDepth = null;
                if (depth > 0)
                    cloneDepth = $" --depth={depth}";

                return ExecuteGitCommand($"clone {gitUrl} \"{localPath}\"{cloneDepth}",
                    timeoutMs: 100000,
                    windowStyle: ProcessWindowStyle.Hidden,
                    progress: progress);
            }
            catch (Exception ex)
            {
                var result = new GitCommandResult
                {
                    HasError = true,
                    Message = "Error cloning repository: " + ex.Message
                };
                return result;
            }
        }


        /// <summary>
        /// Creates a new repository which is the equivalent of a Git Init.
        /// </summary>
        /// <remarks>Note until you make your first commit there's no active branch.</remarks>
        /// <param name="path">Path where to create a repository. Path should not exist yet.</param>        
        /// <param name="gitIgnoreText">Text for the .gitignore file in the Git root</param>
        /// <returns></returns>
        public bool CreateRepository(string path, string gitIgnoreText = null)
        {
            try
            {
                Repository.Init(path, false);
            }
            catch (Exception ex)
            {
                SetError("Error creating repository: " + ex.Message);
                return false;
            }

            var gitIgnoreFile = Path.Combine(path, ".gitignore");
            if (!File.Exists(gitIgnoreFile))
            {
                if (string.IsNullOrEmpty(gitIgnoreText))
                    gitIgnoreText = "*.saved.md\r\n*.bak\r\n*.tmp";

                File.WriteAllText(gitIgnoreFile, gitIgnoreText);
            }
            return true;
        }

        /// <summary>
        /// Checks out a branch on an active repository
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Checkout(string branch, string path)
        {
            var repo = OpenRepository(path);
            if (repo == null)
                return false;

            try
            {
                var gitBranch = Commands.Checkout(repo, branch);
                if (gitBranch == null)
                {
                    SetError($"Couldn't change out branch {branch}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                SetError($"Couldn't change out branch {branch}: {ex.Message}");
                return false;
            }


            return true;
        }

        /// <summary>
        /// Adds a remote to the current Repository.
        ///
        /// Requires that you open a repository first
        /// </summary>
        /// <param name="githubUrl"></param>
        /// <param name="remoteName"></param>
        /// <returns>true or false</returns>
        public bool AddRemote(string githubUrl, string remoteName = "origin")
        {
            if (Repository == null)
            {
                SetError("Repository has to be open before adding a remote.");
                return false;
            }

            string origPath = Environment.CurrentDirectory;
            try
            {
                //git remote add origin https://github.com/RickStrahl/Test.git                
                Repository.Network.Remotes.Add(remoteName, githubUrl);
            }
            catch (Exception ex)
            {
                SetError("Unable to add remote: " + ex.Message);
                return false;
            }

            // try to set the upstream
            var branch = Repository.Head?.FriendlyName;
            if (!string.IsNullOrEmpty(branch))
                ExecuteGitCommand("branch --set-upstream " + Repository.Head.FriendlyName);

            return true;
            //if (Repository == null)
            //{
            //    SetError("Repository has to be open before adding a remote.");
            //    return false;
            //}

            //string origPath = Environment.CurrentDirectory;
            //try
            //{
            //    //git remote add origin https://github.com/RickStrahl/Test.git
            //    Repository.Network.Remotes.Add(remoteName, githubUrl);                
            //}
            //catch (Exception ex)
            //{
            //    SetError("Unable to add remote: " + ex.Message);
            //    return false;
            //}

            //return true;
        }

        public Credential GetGitCredentials(string gitUrl)
        {
            var secrets = new SecretStore("git");

            var auth = new BasicAuthentication(secrets);
            var uri = new Uri(gitUrl);
            var url = uri.Scheme + "://" + uri.Authority;
            var creds = auth.GetCredentials(new TargetUri(url));

            //if (creds == null)
            // TODO: Prompt for

            return creds;
        }

        public bool Commit(ObservableCollection<RepositoryStatusItem> statusItems, string message, string name, string email, bool ammendPreviousCommit = false)
        {
            if (statusItems == null || statusItems.Count < 1)
                return true;


            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
            {
                SetError("Couldn't commit: Git requires a commit name and email address.");
                return false;
            }

            if (Repository == null)
            {
                Repository = OpenRepository(statusItems[0].FullPath);
                if (Repository == null)
                    return false;
            }

            try
            {
                var stageFiles = statusItems
                    .Select(si => si.Filename).ToList();
                if (stageFiles.Count > 0)
                    Commands.Stage(Repository, stageFiles);
            }
            catch (Exception ex)
            {
                SetError($"Couldn't stage changes: {ex.Message}.");
                return false;
            }

            try
            {
                var sig = new Signature(name, email, DateTimeOffset.UtcNow);
                Repository.Commit(message, sig, sig, new CommitOptions
                {
                    AmendPreviousCommit = ammendPreviousCommit
                });
            }
            catch (EmptyCommitException)
            {
                return true;
            }
            catch (Exception ex)
            {
                SetError($"Couldn't commit changes: {ex.Message}.");
                return false;
            }

            return true;
        }

        public async Task<bool> PushAsync(string path, string branch = null)
        {
            return await Task.Run(() => Push(path, branch));
        }

        /// <summary>
        /// Pushes changes to the origin on the remote
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool Push(string path, string branch = null)
        {
            if (string.IsNullOrEmpty(branch))
            {
                using (var repo = OpenRepository(path))
                {
                    branch = repo.Head?.FriendlyName;
                }
            }

            if (!string.IsNullOrEmpty(branch))
                branch = " -u " + branch;

            var result = ExecuteGitCommand("push origin" + branch, path, 60000);
            if (result.HasError)
            {
                SetError("Couldn't push to repository: " + result.Message);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Pulls changes as a
        /// </summary>
        /// <param name="path"></param>
        /// <param name="rebase">If true uses --rebase instead of merging</param>
        /// <returns></returns>
        public bool Pull(string path, bool rebase = false, string branch = "master")
        {
            if (string.IsNullOrEmpty(branch))
            {
                using (var repo = OpenRepository(branch))
                {
                    branch = repo.Head?.TrackedBranch.FriendlyName;
                }
            }


            string rebaseString = null;
            if (rebase)
                rebaseString = " --rebase";

            if (!string.IsNullOrEmpty(branch))
                rebaseString += " " + branch;

            var result = ExecuteGitCommand($"pull origin{rebaseString}", path, 60000);
            if (result.HasError)
            {
                SetError("Couldn't pull from repository: " + result.Message);
                return false;
            }

            return true;
        }

        public async Task<bool> PullAsync(string path, bool rebase = false)
        {
            return await Task.Run<bool>(() =>
            {
                var result = Pull(path, rebase);
                return true;
            });
        }


        /// <summary>
        /// Executes a Git Command on the command line.
        ///
        /// Recommend that you only use this for Remote commands that
        /// require authentication so that the Windows Credentials Store
        /// can handle providing sticky Auth to Github, VSTS and BitBucket.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="timeoutMs"></param>
        /// <param name="windowStyle"></param>
        /// <param name="progress"></param>
        /// <returns></returns>
        public GitCommandResult ExecuteGitCommand(string arguments,
                                                  string path = null,
                                                  int timeoutMs = 10000,
                                                  ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden,
                                                  Action<object, DataReceivedEventArgs> progress = null)
        {
            Process process;
            var result = new GitCommandResult();

            if (string.IsNullOrEmpty(arguments))
                return null;

            arguments = arguments.Trim();

            string oldPath = null;
            if (!string.IsNullOrEmpty(path))
            {
                oldPath = Directory.GetCurrentDirectory();
                Directory.SetCurrentDirectory(path);
            }

            try
            {
                using (process = new Process())
                {
                    process.StartInfo.FileName = "git.exe";
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.WindowStyle = windowStyle;
                    if (windowStyle == ProcessWindowStyle.Hidden)
                        process.StartInfo.CreateNoWindow = true;

                    process.StartInfo.UseShellExecute = false;

                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;

                    DataReceivedEventHandler evh = (s, e) =>
                    {
                        progress(s, e);
                        result.Output += e.Data;
                    };

                    bool hookupProgressEvents = progress != null && timeoutMs > 0;
                    if (hookupProgressEvents)
                    {
                        process.OutputDataReceived += evh;
                        process.ErrorDataReceived += evh;
                    }

                    process.Start();

                    if (hookupProgressEvents)
                    {
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                    }

                    if (timeoutMs < 0)
                        timeoutMs = 99999999; // indefinitely

                    if (timeoutMs > 0)
                    {
                        bool waited = process.WaitForExit(timeoutMs);

                        if (hookupProgressEvents)
                        {
                            process.OutputDataReceived -= evh;
                            process.ErrorDataReceived -= evh;
                        }
                        else
                        {
                            result.Output = process.StandardError.ReadToEnd();
                        }

                        result.ExitCode = process.ExitCode;
                        if (result.ExitCode != 0)
                            result.HasError = true;

                        if (!waited)
                        {
                            result.HasError = true;
                            if (result.ExitCode == 0)
                                result.Message = "Process timed out.";
                        }
                        else if (result.HasError)
                            result.Message = result.Output;

                        if (oldPath != null)
                            Directory.SetCurrentDirectory(oldPath);

                        return result;
                    }


                    result.ExitCode = process.ExitCode;
                }
            }
            catch (Exception ex)
            {
                result.ExitCode = -1;
                result.Message = ex.Message;
            }

            if (oldPath != null)
                Directory.SetCurrentDirectory(oldPath);

            return result;
        }

        #region Helpers



        public static string FindGitRepositoryRoot(string folder)
        {

            if (File.Exists(folder))
                folder = Path.GetDirectoryName(folder);

            if (!Directory.Exists(folder))
                return null;

            var gitHead = Path.Combine(folder, ".git", "HEAD");
            if (File.Exists(gitHead))
                return folder;

            var di = new DirectoryInfo(folder).Parent;
            if (di == null)
                return null;

            return FindGitRepositoryRoot(di.FullName);
        }


        /// <summary>
        /// Opens the configured Diff tool for the provided file and
        /// allows comparison.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public bool OpenDiffTool(string filePath)
        {
            if (!File.Exists(mmApp.Configuration.Git.GitDiffExecutable))
            {
                SetError("There is no diff tool configured. Set the `GitDiffExecutable` setting to your preferred Diff tool.");
                return false;
            }

            var fileText = GetComittedFileTextContent(filePath);
            if (fileText == null)
            {
                SetError("Unable to compare files: " + ErrorMessage);
                return false;
            }

            var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "mm_diff_" + System.IO.Path.GetFileName(filePath));

            File.WriteAllText(tempFile, fileText);

            // Delete files older than 5 minutes
            FileUtils.DeleteTimedoutFiles(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "mm_diff_" + "*.*"), 300);

            ShellUtils.ExecuteProcess(mmApp.Configuration.Git.GitDiffExecutable, $"\"{tempFile}\" \"{filePath}\"");

            return true;
        }


        public const FileStatus DefaultStatusesToDisplay = FileStatus.ModifiedInIndex | FileStatus.ModifiedInWorkdir |
                                                           FileStatus.NewInIndex | FileStatus.NewInWorkdir |
                                                           FileStatus.DeletedFromIndex | FileStatus.DeletedFromWorkdir |
                                                           FileStatus.Conflicted | FileStatus.RenamedInIndex |
                                                           FileStatus.RenamedInWorkdir;

        /// <summary>
        /// Sets the StatusItems property with all changed items
        /// </summary>
        /// <param name="fileOrFolder">File or folder to get changes for</param>
        /// <param name="selectedFile"></param>
        /// <param name="selectAll"></param>
        /// <returns></returns>
        public ObservableCollection<RepositoryStatusItem> GetRepositoryChanges(string fileOrFolder,
            string selectedFile = null,
            bool selectAll = false,
            FileStatus includedStatuses = DefaultStatusesToDisplay)
        {
            Repository = OpenRepository(fileOrFolder);
            if (Repository == null)
                return null;

            var statusItems = new ObservableCollection<RepositoryStatusItem>();

            var status = Repository.RetrieveStatus();

            string relSelectedFile = null;
            if (!string.IsNullOrEmpty(selectedFile))
                relSelectedFile = FileUtils.GetRelativePath(selectedFile, Repository.Info.WorkingDirectory);

            foreach (var item in status)
            {
                if (!includedStatuses.HasFlag(item.State))
                    continue;

                var statusItem = new RepositoryStatusItem
                {
                    Filename = item.FilePath,
                    FullPath = Path.Combine(Repository.Info.WorkingDirectory, item.FilePath),
                    FileStatus = item.State,
                };

                if (selectAll || relSelectedFile != null && relSelectedFile.Equals(statusItem.Filename, StringComparison.InvariantCultureIgnoreCase))
                    statusItem.Selected = true;

                statusItems.Add(statusItem);
            }

            //statusItems = new ObservableCollection(

            statusItems = new ObservableCollection<RepositoryStatusItem>(statusItems
                    .OrderByDescending(si => si.Selected)
                    .ThenBy(si => Path.GetDirectoryName(si.Filename))
                    .ThenBy(si => Path.GetFileName(si.Filename)));

            return statusItems;
        }


        /// <summary>
        /// Returns the text content for the last commit of a file
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public string GetComittedFileTextContent(string path)
        {
            var repo = OpenRepository(path);
            if (repo == null)
            {
                SetError("No repository found.");
                return null;
            }

            var relPath = FileUtils.GetRelativePath(path, repo.Info.WorkingDirectory);

            TreeEntry entry;
            try
            {
                entry = repo.Head.Tip.Tree[relPath];
            }
            catch
            {
                SetError("Unable to find item in last commit.");
                return null;
            }

            if (entry == null)
            {
                SetError("File has no previous commit data.");
                return null;
            }

            var blob = entry.Target as Blob;
            string text = null;
            try
            {
                text = blob.GetContentText();
            }
            catch
            {
                SetError("Not text content.");
                return null;
            }

            return text;
        }


        /// <summary>
        /// Adds an entry to the root .gitignore file if the
        /// the value doesn't already exist
        /// </summary>
        /// <param name="filePath">Path to add</param>
        /// <returns></returns>
        public bool IgnoreFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                SetError("File to ignore doesn't exist.");
                return false;
            }

            var repo = OpenRepository(filePath);
            if (repo == null)
            {
                SetError("No repository found to ignore in.");
                return false;
            }

            var gitIgnoreFile = Path.Combine(repo.Info.WorkingDirectory, ".gitignore");

            string content = string.Empty;
            if (File.Exists(gitIgnoreFile))
                content = File.ReadAllText(gitIgnoreFile);

            var relPath = FileUtils.GetRelativePath(filePath, repo.Info.WorkingDirectory).Replace("\\", "/");


            if (CultureInfo.InvariantCulture.CompareInfo.IndexOf(content, relPath + "\r") < 0 &&
                CultureInfo.InvariantCulture.CompareInfo.IndexOf(content, relPath + "\n") < 0)
            {
                content = content.TrimEnd() + "\r\n" + relPath + "\r\n";
                File.WriteAllText(gitIgnoreFile, content);
            }

            return true;
        }


        /// <summary>
        /// Retrieves Username and Email as a two item array from .gitconfig
        /// file if it exists.                
        /// </summary>
        /// <returns>
        /// Two Item String Array. Items: 0 - Username, 1 - Email
        /// </returns>
        public static string[] GetGitNameAndEmailFromGitConfig()
        {
            string email = string.Empty;
            string username = string.Empty;

            var gitignoreFile = Path.Combine(Environment.GetEnvironmentVariable("userprofile"), ".gitconfig");
            if (File.Exists(gitignoreFile))
            {
                var fileText = File.ReadAllText(gitignoreFile);

                username = StringUtils.ExtractString(fileText, "name = ", "\n");
                email = StringUtils.ExtractString(fileText, "email = ", "\n");
            }

            return new string[2] { username.Trim(), email.Trim() };
        }
        #endregion

        #region Git Locations and Installation
        /// <summary>
        /// Checks to see if Git is installed on the local machine
        /// </summary>
        /// <returns></returns>
        public static string FindGitExecutable()
        {
            string exe = Path.Combine(Environment.GetEnvironmentVariable("Program6432"), "Git\\bin\\git.exe");
            if (!File.Exists(exe))
                return exe;

            exe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                "Git\\bin\\git.exe");
            if (!File.Exists(exe))
                return exe;

            return null;
        }

        /// <summary>
        /// Determines whether Git is installed
        /// </summary>
        /// <returns></returns>
        public static bool IsGitInstalled()
        {
            return FindGitExecutable() != null;
        }

        /// <summary>
        /// Navigates browser to the Git Web download location
        /// </summary>
        public static void GotoGitDownload()
        {
            ShellUtils.GoUrl("https://git-scm.com/download/win");
        }

        #endregion

        #region Error Handling

        public string ErrorMessage { get; set; }

        protected void SetError()
        {
            SetError("CLEAR");
        }

        protected void SetError(string message)
        {
            if (message == null || message == "CLEAR")
            {
                ErrorMessage = string.Empty;
                return;
            }
            ErrorMessage = message;
        }

        protected void SetError(Exception ex, bool checkInner = false)
        {
            if (ex == null)
                this.ErrorMessage = string.Empty;

            Exception e = ex;
            if (checkInner)
                e = e.GetBaseException();

            ErrorMessage = e.Message;
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Repository?.Dispose();
        }
        #endregion
    }

    [DebuggerVisualizer("{Message}")]
    public class GitCommandResult
    {
        public bool HasError { get; set; }
        public int ExitCode { get; set; }
        public string Message { get; set; }
        public string Output { get; set; }
    }


    [DebuggerVisualizer("{FileName} - {FileStatus}")]
    public class RepositoryStatusItem : INotifyPropertyChanged
    {
        public string Filename { get; set; }

        public string FullPath { get; set; }


        public FileStatus FileStatus
        {
            get => _fileStatus;
            set
            {
                if (value == _fileStatus) return;
                _fileStatus = value;
                OnPropertyChanged();
            }
        }
        private FileStatus _fileStatus;

        public bool Selected
        {
            get => _selected;
            set
            {
                if (value == _selected) return;
                _selected = value;
                OnPropertyChanged();
            }
        }
        private bool _selected;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
