using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkdownMonster.AddIns;
using MarkdownMonster.Windows;

namespace MarkdownMonster
{
    public class AppCommands
    {
        AppModel Model;

        public AppCommands(AppModel model)
        {
            Model = model;

            // File Operations
            NewWeblogPost();


            // Configuration 
            OpenAddinManager();

            // Misc
            OpenSampleMarkdown();
            OpenRecentDocument();
        }
        
        public CommandBase NewWeblogPostCommand { get; set; }

        void NewWeblogPost()
        {
            NewWeblogPostCommand = new CommandBase((parameter, command) =>
            {
                
                AddinManager.Current.RaiseOnNotifyAddin("newweblogpost", null);
            });
        }



        public CommandBase AddinManagerCommand { get; set; }

        void OpenAddinManager()
        {
            AddinManagerCommand = new CommandBase((parameter, command) =>
            {
                var form = new AddinManagerWindow
                {
                    Owner = Model.Window
                };
                form.Show();
            });
        }


        public CommandBase OpenSampleMarkdownCommand { get; set; }

        void OpenSampleMarkdown()
        {
            OpenSampleMarkdownCommand = new CommandBase((parameter, command) =>
            {
                string tempFile = Path.Combine(Path.GetTempPath(), "SampleMarkdown.md");
                File.Copy(Path.Combine(Environment.CurrentDirectory, "SampleMarkdown.md"), tempFile, true);
                Model.Window.OpenTab(tempFile);
            });
        }


        public CommandBase OpenRecentDocumentCommand { get; set; }
        
        void OpenRecentDocument()
        {
            OpenRecentDocumentCommand = new CommandBase((parameter, command) =>
            {
                var parm = parameter as string;
                if (parm == null)
                    return;

                Model.Window.OpenTab(parm);
            },(p,c)=> true);
        }


    }
}
