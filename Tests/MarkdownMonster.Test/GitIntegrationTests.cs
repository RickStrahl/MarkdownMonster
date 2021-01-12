using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using MarkdownMonster.Utilities;
using MarkdownMonster.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.Utilities;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class GitIntegrationTests
    {
        [TestMethod]
        public void OpenLocalRepoTest()
        {
            using (var repo = new LibGit2Sharp.Repository(@"c:\\projects\\markdownmonster"))
            {
                Assert.IsNotNull(repo);
            }
        }


        [TestMethod]
        public void ListChangesRepoTest()
        {
            string path =  @"c:\projects\markdownmonster\markdownmonster";
            var helper = new GitHelper();
            using (var repo = helper.OpenRepository(path))
            {
                Assert.IsNotNull(repo);

                var folder = new FolderStructure(null);
                var pathItem = folder.GetFilesAndFolders(path);
            
                folder.UpdateGitFileStatus(pathItem);
            }
        }


        [TestMethod]
        public void GetGitStatusForFileTest()
        {
            string path = @"c:\projects\markdownmonster\markdownmonster";

            var helper = new GitHelper();
            var status = helper.GetGitStatusForFile(Path.Combine(path, "MarkdownMonster.csproj"));

            Console.WriteLine(status);
        }

        [TestMethod]
        public void GetCommittedFileContents()
        {

            string path = @"c:\Projects\markdownmonster";

            var helper = new GitHelper();
            var fileData = helper.GetComittedFileTextContent(Path.Combine(path, "README.md"));
            Assert.IsNotNull(fileData);
        }

        [TestMethod]
        public void GetGitStatusForRepo()
        {
            string path = @"c:\Projects\markdownmonster";

            var helper = new GitHelper();
            var statusItems = helper.GetRepositoryChanges(path,null,true);
            Assert.IsNotNull(statusItems);

            foreach (var item in statusItems)
            {
                Console.WriteLine($"{item.Filename} - {item.FileStatus}");
            }
        }

        [TestMethod]
        public void CommitTest()
        {
            string Path = @"c:\temp\MarkdownMonsterGitTest";

            var helper = new GitHelper();

            var changes = helper.GetRepositoryChanges(Path, selectAll: true);

            helper.Commit(changes, "Committing changes...","Rick Strahl","rstrahl@test.com");
        }


        [TestMethod]
        public void OpenRemoteRepoTest()
        {
            const string localFolder = "C:/temp/MarkdownMonster/";
            const string remote = "https://github.com/RickStrahl/MarkdownMonster.git";

            var destination = Repository.Init(localFolder,false);
            var repo = new Repository(destination);
            var branch = repo.CreateBranch("master");
            if (repo.Network.Remotes.Count() == 0)
            {
                repo.Network.Remotes.Add("origin", remote);
            }

            var mergeResult = LibGit2Sharp.Commands.Pull(repo,
                new Signature("rickstrahl",
                    "rstrahl@west-wind.com", new DateTimeOffset(DateTime.Now)),
                    new PullOptions());                     

            Assert.IsNotNull(mergeResult);
        }


        [TestMethod]
        public void CloneRepo()
        {
            Repository.Clone("https://github.com/RickStrahl/MarkdownMonster.git", "c:\\temp\\MarkdownMonster");
        }


        [TestMethod]
        public void CreateRepository()
        {
            var path = @"c:\temp\GithubRepos\testRepo2";

            if(Directory.Exists(path))
                Directory.Delete(path, true);

            var gh = new GitHelper();
            bool result = gh.CreateRepository(path,"*.saved.md\r\n*.bak\r\n*.tmp");            

            Assert.IsTrue(result);
            Assert.IsTrue(Directory.Exists(path));


            var fileToAdd = Path.Combine(path, "test.txt");

            var list = new ObservableCollection<RepositoryStatusItem>()
            {
                new RepositoryStatusItem { Filename  = fileToAdd }
            };

            gh.OpenRepository(path);
            Assert.IsTrue(gh.Commit(list, "first commit", "ras", "r@west-wind.com"), gh.ErrorMessage);

            ShellUtils.OpenFileInExplorer(path);
        }


        [TestMethod]
        public void CreateAndAddRemoteTest()
        {

            var path = @"c:\temp\GithubRepos\testRepo2";

            if (Directory.Exists(path))
                Directory.Delete(path, true);

            var gh = new GitHelper();
            bool result = gh.CreateRepository(path, "*.saved.md\r\n*.bak\r\n*.tmp");

            Assert.IsTrue(result);
            Assert.IsTrue(Directory.Exists(path));


            var fileToAdd = Path.Combine(path, "test.txt");
            File.WriteAllText(fileToAdd, "test");

            var list = new ObservableCollection<RepositoryStatusItem>()
            {
                new RepositoryStatusItem { Filename  = fileToAdd }
            };

            gh.OpenRepository(path);
            Assert.IsTrue(gh.Commit(list, "first commit", "ras", "r@west-wind.com"), gh.ErrorMessage);

            Assert.IsTrue(gh.AddRemote("https://github.com/RickStrahl/Test5.git", "origin"), gh.ErrorMessage);

            Assert.IsTrue(gh.Push(path,"master"), gh.ErrorMessage);

            ShellUtils.OpenFileInExplorer(path);

        }

        [TestMethod]
        public void CreateRespositoryRaw()
        {
            var path = @"c:\temp\testrepos\TestRepo3";

            // works - path and .git folder created
            Repository.Init(path, false);

            var fileToAdd = Path.Combine(path, "test.txt");
            File.WriteAllText(fileToAdd, "test");

            using (var repo = new Repository(path))
            {
                Commands.Stage(repo,fileToAdd);

                var sig = new Signature("ras", "r@west-wind.com", DateTimeOffset.Now);
                repo.Commit("committing test.txt", sig, sig);

                var branch = repo.Branches["master"];
                if (branch == null)
                    branch = repo.CreateBranch("master");

                Commands.Checkout(repo, branch, new CheckoutOptions {CheckoutModifiers = CheckoutModifiers.Force});
            }


        }

    }
}
