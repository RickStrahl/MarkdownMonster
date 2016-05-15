using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FontAwesome.WPF;
using MarkdownMonster;
using MarkdownMonster.AddIns;
using Westwind.Utilities;

namespace SnagItAddin
{
    public class SnagitAddin : MarkdownMonsterAddin, IMarkdownMonsterAddin
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

            this.MenuItems.Add(menuItem);
        }

        public void SnagitMenu_Execute(object sender)
        {
            SnagItAutomation SnagIt = SnagItAutomation.Create();
            SnagIt.ActiveForm = Model.Window;
            var editor = Model.Window.GetActiveMarkdownEditor();

            SnagIt.CapturePath = editor?.MarkdownDocument.Filename;
            if (!string.IsNullOrEmpty(SnagIt.CapturePath))
                SnagIt.CapturePath = Path.GetDirectoryName(SnagIt.CapturePath);

            //SnagItConfigurationForm ConfigForm = new SnagItConfigurationForm(SnagIt);
            //if (ConfigForm.ShowDialog() == DialogResult.Cancel)
            //    return DialogResult.Cancel;

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
        
            
        
    }
}
