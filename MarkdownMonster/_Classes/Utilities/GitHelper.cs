using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace MarkdownMonster.Utilities
{
    /// <summary>
    /// Helps with a number of Git Operations
    /// </summary>
    public class GitHelper : IDisposable
    {
        public Repository Repository { get; set; }

        public Func<string,bool> CloneProgress { get; set; }
        
        /// <summary>
        /// Opens a repository and stores it in the Repository 
        /// property of this class
        /// </summary>
        /// <param name="localPath"></param>
        /// <returns></returns>
        public Repository OpenRepository(string localPath)
        {
            try
            {
                Repository = new Repository(localPath);
                return Repository;
            }
            catch(Exception ex)
            {
                SetError(ex);               
            }

            return null;
        }


        /// <summary>
        /// Clones a repository
        /// </summary>
        /// <param name="githubUrl"></param>
        /// <param name="localPath"></param>
        /// <param name="addAsRemote"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool CloneRepository(string githubUrl,
                string localPath,                
                string username = null,
                string password = null                
            )
        {

            Repository repo = null;
            try
            {
                var options = new CloneOptions
                {
                    Checkout = true,
                    BranchName = "master"                    
                };

                
                if (CloneProgress != null)
                    options.OnProgress = new LibGit2Sharp.Handlers.ProgressHandler(CloneProgress);
                
                if (!string.IsNullOrEmpty(username))
                {
                    options.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials()
                    {
                        Username = username,
                        Password = password
                    };
                }               

               Repository.Clone(githubUrl, localPath,options);
            }
            catch (Exception ex)
            {
                SetError(ex);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Adds a remote to the current Repository.
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
                SetError("Unable to add remote.");
                return false;
            }            

            return true;
        }
        

        #region Error Handling

        public string ErrorMessage { get; set; }

        protected void SetError()
        {
            this.SetError("CLEAR");
        }

        protected void SetError(string message)
        {
            if (message == null || message == "CLEAR")
            {
                this.ErrorMessage = string.Empty;
                return;
            }
            this.ErrorMessage += message;
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
}
