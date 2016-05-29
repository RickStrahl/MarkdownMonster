using System;
using System.IO;
using System.Windows.Controls;
using FontAwesome.WPF;
using MarkdownMonster.AddIns;
using Westwind.Utilities;

namespace SnagItAddin
{

    public class SnagitAddin : MarkdownMonsterAddin
    {
        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            // Add a menu item
            var menuItem = new AddInMenuItem()
            {                
                Caption = "SnagIt Screen Capture",
                FontawesomeIcon= FontAwesomeIcon.Camera,

                // a unique command id that is tied to the menuitem
                EditorCommand = "snagit",
            };
            menuItem.Execute = new Action<object>(SnagitMenu_Execute);
            menuItem.ExecuteConfiguration = new Action<object>(SnagitConfigurationMenu_Execute);
            menuItem.CanExecute = new Func<object,bool>(SnagitConfigurationMenu_CanExecute);
            
            this.MenuItems.Add(menuItem);
        }

        public void SnagitMenu_Execute(object sender)
        {
            var config = ScreenCaptureConfiguration.Current;

            if (config.AlwaysShowCaptureOptions)
            {
                var form = new ScreenCaptureConfigurationForm()
                {
                    Owner = Model.Window,
                    IsPreCaptureMode = true
                };

                var result = form.ShowDialog();
                if (result == null || !result.Value)
                    return;
            }


            SnagItAutomation SnagIt = SnagItAutomation.Create();

            
            var editor = Model.Window.GetActiveMarkdownEditor();

            SnagIt.CapturePath = editor?.MarkdownDocument.Filename;
            if (!string.IsNullOrEmpty(SnagIt.CapturePath))
                SnagIt.CapturePath = Path.GetDirectoryName(SnagIt.CapturePath);

                    
            string capturedFile = SnagIt.CaptureImageToFile();
            if (string.IsNullOrEmpty(capturedFile) || !File.Exists(capturedFile))
                return;

            capturedFile = FileUtils.GetRelativePath(capturedFile, SnagIt.CapturePath);
            string relPath = capturedFile.Replace("\\", "/");
            if (relPath.StartsWith(".."))
                relPath = capturedFile;

            string replaceText = "![](" +  relPath + ")";
            
            
            // Push the new text into the Editor's Selection
            this.SetSelection(replaceText);
        }

        public void SnagitConfigurationMenu_Execute(object sender)
        {
            var configForm = new ScreenCaptureConfigurationForm()
            {
                Owner = this.Model.Window
            };            
            configForm.Show();
        }

        public bool SnagitConfigurationMenu_CanExecute(object sender)
        {            
            if (!SnagItAutomation.IsInstalled)
            {
                var button = sender as Button;
                button.ToolTip = "SnagIt isn't installed. Currently only SnagIt based captures are supported.";
                button.IsEnabled = false;    
                return false;
            }

            return true;
        }
    }
}
