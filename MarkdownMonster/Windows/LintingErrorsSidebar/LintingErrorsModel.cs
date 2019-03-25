using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MarkdownMonster;
using MarkdownMonster.Annotations;
using Newtonsoft.Json.Linq;

namespace MarkdownMonster.Windows
{
    public class LintingErrorsModel : INotifyPropertyChanged
    {
        private List<MarkdownLintError> _lintingErrors;
        public event PropertyChangedEventHandler PropertyChanged;

        public AppModel AppModel { get; set; }

        public List<MarkdownLintError> LintingErrors
        {
            get => _lintingErrors;
            set
            {
                if (Equals(value, _lintingErrors)) return;
                _lintingErrors = value;
                OnPropertyChanged();
            }
        }


        public LintingErrorsModel()
        {
            AppModel = mmApp.Model;
        }

        /// <summary>
        /// Gets linting errors from the current document
        /// </summary>
        /// <returns></returns>
        public List<MarkdownLintError> GetLintingErrors()
        {
            if (AppModel.Configuration.MarkdownOptions.MarkdownLinting && AppModel.ActiveEditor != null)
                LintingErrors = MarkdownLinting(AppModel.ActiveEditor.GetMarkdown());
            else
                return null;

            return LintingErrors;
        }


        public static List<MarkdownLintError> MarkdownLinting(string markdown, bool noErrorUi = false)
        {
            // Old code that uses JavaScript in a WebBrowser Control
            string htmlFile = Path.Combine(App.InitialStartDirectory, "Editor\\markdownlinting.htm");

            var form = new BrowserDialog();
            form.ShowInTaskbar = false;
            form.ShowActivated = false;
            form.Width = 1;
            form.Height = 1;
            form.Left = -20000;
            form.Show();

            bool exists = File.Exists(htmlFile);
            form.NavigateAndWaitForCompletion(htmlFile);

            WindowUtilities.DoEvents();

            var errorList = new List<MarkdownLintError>();
            try
            {
                dynamic doc = form.Browser.Document;
                string result = doc.ParentWindow.markdownlinting(markdown) as string;

                // ClipboardHelper.SetText(result);

                dynamic root = JObject.Parse(result);
                if (root == null)
                    return null;

                JArray errors = root.content as JArray;
                if (errors == null)
                    return null;

                foreach (dynamic error in errors)
                {
                    var lintError = new MarkdownLintError
                    {
                        Description = error.ruleDescription,
                        Detail = error.errorDetail,
                        LineNumber = error.lineNumber
                    };

                    JArray ruleNames = error.ruleNames;
                    if (ruleNames.Count > 0)
                    {
                        lintError.RuleName = ruleNames[0].ToString();
                    }

                    errorList.Add(lintError);
                }

                errorList = errorList.OrderBy(el => el.LineNumber).ToList();
            }
            catch (Exception ex)
            {
                mmApp.Log("Failed to lint Markdown text", ex);
                if (!noErrorUi)
                    MessageBox.Show("Unable to lint Markdown.", "Markdown Linting Failed.", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            form.Close();

            return errorList;
        }


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
