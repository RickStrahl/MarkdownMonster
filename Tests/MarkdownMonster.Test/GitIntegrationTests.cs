using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using MarkdownMonster.Utilities;
using MarkdownMonster.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class GitIntegrationTests
    {
        [TestMethod]
        public void OpenLocalRepoTest()
        {
            using (var repo = new LibGit2Sharp.Repository(@"c:\\projects2010\\markdownmonster"))
            {
                Assert.IsNotNull(repo);
            }
        }


        [TestMethod]
        public void ListChangesRepoTest()
        {
            string path =  @"c:\projects2010\markdownmonster\markdownmonster";
            var helper = new GitHelper();
            using (var repo = helper.OpenRepository(path))
            {
                Assert.IsNotNull(repo);

                var folder = new FolderStructure();
                var pathItem = folder.GetFilesAndFolders(path);
            
                folder.UpdateGitFileStatus(pathItem);
            }
        }


        [TestMethod]
        public void GetGitStatusForFileTest()
        {
            string path = @"c:\projects2010\markdownmonster\markdownmonster";

            var helper = new GitHelper();
            var status = helper.GetGitStatusForFile(Path.Combine(path, "MarkdownMonster.csproj"));

            Console.WriteLine(status);
        }

        [TestMethod]
        public void GetCommittedFileContents()
        {

            string path = @"c:\projects2010\markdownmonster";

            var helper = new GitHelper();
            var fileData = helper.GetComittedFileTextContent(Path.Combine(path, "README.md"));
            Assert.IsNotNull(fileData);
        }

        [TestMethod]
        public void GetGitStatusForRepo()
        {
            string path = @"c:\projects2010\markdownmonster";

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

    }
}
