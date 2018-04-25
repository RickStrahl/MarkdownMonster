using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Alm.Authentication;

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
        /// <param name="gitUrl"></param>
        /// <param name="localPath"></param>
        /// <param name="addAsRemote"></param>
        /// <param name="useGitCredentialManager"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool CloneRepository(string gitUrl,
                string localPath,
                bool useGitCredentialManager = false,
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
                else if(!string.IsNullOrEmpty(username))
                {
                    
                    options.CredentialsProvider = (_url, _user, _cred) => new UsernamePasswordCredentials()
                    {
                        Username = username,
                        Password = password
                    };
                }

                Repository.Clone(gitUrl, localPath,options);
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
        /// <returns></returns>
        public GitCommandResult CloneRepositoryCommandLine(string gitUrl, string localPath, Action<object, DataReceivedEventArgs> progress = null)
        {
            try
            { 
                return ExecuteGitCommand($"clone {gitUrl} {localPath}",
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

        public GitCommandResult ExecuteGitCommand(string arguments,
                                                  int timeoutMs=10000,
                                                  ProcessWindowStyle windowStyle= ProcessWindowStyle.Hidden,
                                                  Action<object,DataReceivedEventArgs>
                                                  progress = null)
        {
            Process process;
            var result = new GitCommandResult();

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
                        else if(result.HasError)
                            result.Message = result.Output;

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

            return result;
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


    public class GitCommandResult
    {
        public bool HasError { get; set; }
        public int ExitCode { get; set; }
        public string Message { get; set; }
        public string Output { get; set; }
    }
}
