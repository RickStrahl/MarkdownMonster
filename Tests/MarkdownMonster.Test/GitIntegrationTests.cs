using System;
using System.Diagnostics;
using System.Linq;
using LibGit2Sharp;
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

    }
}
