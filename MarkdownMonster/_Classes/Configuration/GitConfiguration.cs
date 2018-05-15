using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkdownMonster.Configuration
{
    public class GitConfiguration
    {

        /// <summary>
        /// Determines how Git Commits are handled - either just commit
        /// or Commit and Push
        /// </summary>
        public GitCommitBehaviors GitCommitBehavior { get; set; } = GitCommitBehaviors.CommitAndPush;

        public bool CloseAfterCommit { get; set; } = true;
        
        /// <summary>
        /// Point to your favorite Git Client to open for folders
        /// </summary>
        public string GitClientExecutable { get; set; }

        /// <summary>
        /// Point to your favorit Git Diff Engine to compare files
        /// </summary>
        public string GitDiffExecutable { get; set; }

        /// <summary>
        /// Git name used for commits
        /// </summary>
        public string GitName { get; set; }

        /// <summary>
        /// Git Email used for commits
        /// </summary>
        public string GitEmail { get; set; }

    }
}
