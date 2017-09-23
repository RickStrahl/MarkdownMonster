using System;
using System.Collections.Generic;
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
            Command_NewWeblogPost();


            // Configuration 
            Command_AddinManager();
        }
        
        public CommandBase NewWeblogPostCommand { get; set; }

        void Command_NewWeblogPost()
        {
            NewWeblogPostCommand = new CommandBase((parameter, command) =>
            {
                AddinManager.Current.RaiseOnNotifyAddin("newweblogpost", null);
            });
        }



        public CommandBase AddinManagerCommand { get; set; }

        void Command_AddinManager()
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


    }
}
