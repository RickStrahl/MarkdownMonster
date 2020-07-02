using System;
using System.IO;
using System.Linq;
using MarkdownMonster.Utilities;
using MarkdownMonster.Windows;
using Westwind.Utilities;

namespace MarkdownMonster
{
    public class GitCommands
    {
        private AppModel Model;

        public GitCommands(AppModel model)
        {
            Model = model;

            OpenFromGitRepo();
            CommitToGit();
            OpenGitClient();
            OpenOnGithub();
        }

        public CommandBase OpenFromGitRepoCommand { get; set; }

        void OpenFromGitRepo()
        {
            OpenFromGitRepoCommand = new CommandBase((parameter, command) =>
            {
                GitRepositoryWindowMode startupMode = GitRepositoryWindowMode.Clone;
                var parm = parameter as string;
                if (!string.IsNullOrEmpty(parm))
                    Enum.TryParse(parm, out startupMode);

                var form = new GitRepositoryWindow(startupMode);
                form.Owner = Model.Window;

                var curPath = Model.Window.FolderBrowser.FolderPath;
                if (string.IsNullOrEmpty(curPath))
                {
                    curPath = Model.ActiveDocument?.Filename;
                    if (!string.IsNullOrEmpty(curPath))
                        curPath = Path.GetDirectoryName(curPath);
                }

                if (!string.IsNullOrEmpty(curPath))
                {
                    if (startupMode == GitRepositoryWindowMode.AddRemote)
                    {
                        var root = GitHelper.FindGitRepositoryRoot(curPath);
                        if (root != null)
                            form.LocalPath = root;
                    }
                    else if (startupMode == GitRepositoryWindowMode.Create)
                    {
                        form.LocalPath = curPath;
                    }
                }

                form.ShowDialog();
            });
            OpenFromGitRepoCommand.PremiumFeatureName = "Git Support";
            OpenFromGitRepoCommand.PremiumFeatureLink = "https://markdownmonster.west-wind.com/docs/_4xp0yygt2.htm";
        }


        public CommandBase CommitToGitCommand { get; set; }

        void CommitToGit()
        {
            // COMMIT TO GIT Command
            CommitToGitCommand = new CommandBase((parameter, e) =>
            {
                var file = parameter as string;
                if (string.IsNullOrEmpty(file))
                    file = Model.ActiveDocument?.Filename;

                if (string.IsNullOrEmpty(file))
                    return;

                var gh = new GitHelper();
                var repo = gh.OpenRepository(file);
                if (repo == null)
                {
                    Model.Window.ShowStatusError("This file or folder is not in a Git repository.");
                    return;
                }

                var changes = gh.GetRepositoryChanges(repo.Info.WorkingDirectory);
                if (changes == null && !string.IsNullOrEmpty(gh.ErrorMessage))
                {
                    Model.Window.ShowStatusError($"Unable to retrieve Repository status: {gh.ErrorMessage}");
                    return;
                }
                if (changes.Count < 1)
                    Model.Window.ShowStatusError(
                        $"There are no pending changes for this Git repository: {repo.Info.WorkingDirectory}");

                if (Model.ActiveEditor != null)
                    Model.ActiveEditor.SaveDocument(Model.ActiveDocument.IsEncrypted);

                var form = new GitCommitDialog(file, false); // GitCommitFormModes.ActiveDocument);
                form.Show();
            }, (s, e) => Model.IsEditorActive);
            CommitToGitCommand.PremiumFeatureName = "Git Support";
            CommitToGitCommand.PremiumFeatureLink = "https://markdownmonster.west-wind.com/docs/_4xp0yygt2.htm";
        }

        public CommandBase OpenGitClientCommand { get; set; }

        void OpenGitClient()
        {
            OpenGitClientCommand = new CommandBase((parameter, command) =>
            {
                var path = parameter as string;
                if (path == null)
                {
                    path = Model.ActiveTabFilename;
                    if (!string.IsNullOrEmpty(path))
                        path = Path.GetDirectoryName(path);
                }

                if (string.IsNullOrEmpty(path))
                    return;

                if (!mmFileUtils.OpenGitClient(path))
                    Model.Window.ShowStatusError("Unabled to open Git client.");
                else
                    Model.Window.ShowStatus("Git client opened.", mmApp.Configuration.StatusMessageTimeout);
            }, (p, c) => !string.IsNullOrEmpty(Model.Configuration.Git.GitClientExecutable));
            OpenGitClientCommand.PremiumFeatureName = "Git Support";
            OpenGitClientCommand.PremiumFeatureLink = "https://markdownmonster.west-wind.com/docs/_4xp0yygt2.htm";
        }


        public CommandBase OpenOnGithubCommand { get; set; }

        void OpenOnGithub()
        {
            OpenOnGithubCommand = new CommandBase((parameter, command) =>
            {
                var filename = parameter as string;
                if (parameter == null)
                    return;

                var CommitModel = new GitCommitModel(filename);

                using (var repo = CommitModel.GitHelper.OpenRepository(CommitModel.Filename))
                {
                    var remoteUrl = repo?.Network.Remotes.FirstOrDefault()?.Url;
                    if (remoteUrl == null)
                        return;

                    var relativeFilename = FileUtils.GetRelativePath(filename, repo.Info.WorkingDirectory)
                        .Replace("\\", "/");

                    remoteUrl = remoteUrl.Replace(".git", "");
                    remoteUrl += "/blob/master/" + relativeFilename;

                    Model.Window.ShowStatus("Opening Url: " + remoteUrl);
                    ShellUtils.GoUrl(remoteUrl);
                }
            });
        }
    }
}
