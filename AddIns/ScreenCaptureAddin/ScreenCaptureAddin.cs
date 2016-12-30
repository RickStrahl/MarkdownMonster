#region License
/*
 **************************************************************
 *  Author: Rick Strahl 
 *          © West Wind Technologies, 2016
 *          http://www.west-wind.com/
 * 
 * Created: 05/15/2016
 *
 * Permission is hereby granted, free of charge, to any person
 * obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use,
 * copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following
 * conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 * WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 * OTHER DEALINGS IN THE SOFTWARE.
 **************************************************************  
*/
#endregion

using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using FontAwesome.WPF;
using MarkdownMonster;
using MarkdownMonster.AddIns;
using MarkdownMonster.Windows;
using Westwind.Utilities;

namespace SnagItAddin
{

    public class ScreenCaptureAddin : MarkdownMonsterAddin
    {
        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            Id = "screencapture";

            // create menu item and use OnExecute/OnExecuteConfiguration/OnCanExecute handlers            
            var menuItem = new AddInMenuItem(this)
            {
                Caption = "SnagIt Screen Capture",
                FontawesomeIcon = FontAwesomeIcon.Camera
            };
            MenuItems.Add(menuItem);
        }

        public override void OnExecute(object sender)
        {
            if (Model.ActiveDocument == null)
            {
                MessageBox.Show("Can't capture a screen - there's no open document to capture to.",
                    mmApp.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SnagItAutomation.IsInstalled && ScreenCaptureConfiguration.Current.UseSnagItForImageCapture)
                ExecuteSnagitCapture();
            else
                ExecuteApplicationFormCapture();

        }

        private void ExecuteSnagitCapture()
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
            SnagIt.ActiveForm = Model.Window;

            var editor = Model.Window.GetActiveMarkdownEditor();

            SnagIt.CapturePath = editor?.MarkdownDocument.Filename;
            SnagIt.CapturePath = !string.IsNullOrEmpty(SnagIt.CapturePath) && SnagIt.CapturePath != "untitled" ? 
                Path.GetDirectoryName(SnagIt.CapturePath) :
                mmApp.Configuration.LastImageFolder;


            string capturedFile = SnagIt.CaptureImageToFile();
            if (string.IsNullOrEmpty(capturedFile) || !File.Exists(capturedFile))
                return;

            capturedFile = FileUtils.GetRelativePath(capturedFile, SnagIt.CapturePath);
            string relPath = capturedFile.Replace("\\", "/");
            if (relPath.StartsWith(".."))
                relPath = capturedFile;

            if (relPath.Contains(":\\"))
                relPath = "file:///" + relPath.Replace("\\", "/");

            string replaceText = "![](" + relPath + ")";

            mmApp.Configuration.LastImageFolder = SnagIt.CapturePath;

            // Push the new text into the Editor's Selection
            SetSelection(replaceText);
        }

        private void ExecuteApplicationFormCapture()
        {
            string imageFolder = Path.GetDirectoryName(Model.ActiveDocument.Filename);

            var form = new ScreenCaptureForm()
            {
                // hide the main window on captures                
                SaveFolder = imageFolder,
                Top = Model.Window.Top,
                Left = Model.Window.Left + 80,
                Height = ScreenCaptureConfiguration.Current.WindowHeight,
                Width = ScreenCaptureConfiguration.Current.WindowWidth
            };

            Model.Window.Hide();

            form.ShowDialog();

            Model.Window.Show();
            Model.Window.Activate();

            Model.Window.Topmost = true;
            WindowUtilities.DoEvents();
            Model.Window.Topmost = false;

            if (form.Cancelled)
            {
                SetSelection("");
                return;
            }

            string capturedFile = form.SavedImageFile;

            try
            {
                capturedFile = FileUtils.GetRelativePath(capturedFile, imageFolder);
            }
            catch
            {
            }

            string relPath = capturedFile;
            if (relPath.StartsWith(".."))
                relPath = form.SavedImageFile;

            if (relPath.Contains(":\\")) // full path
                relPath = "file:///" + relPath;

            string replaceText = "![](" + relPath + ")";

            // Push the new text into the Editor's Selection
            SetSelection(replaceText);
            Model.ActiveEditor.SetEditorFocus();
            Model.Window.PreviewMarkdown(Model.ActiveEditor, true);

        }

#if false
        private void ExecuteExternalFormCapture()
        {
            // Result file holds filename in temp folder
            string resultFilePath = Path.Combine(Path.GetTempPath(), "ScreenCapture.result");
            string imageFolder = null;

            imageFolder = Path.GetDirectoryName(Model.ActiveDocument.Filename);


            var helper = new WindowInteropHelper((Window)Model.Window);
            var handle = helper.Handle;

            if (File.Exists(resultFilePath))
                File.Delete(resultFilePath);

            string args = $@"""{imageFolder}"" {handle} {Model.Window.Left} {Model.Window.Top}";

            try
            {
                var process = Process.Start(new ProcessStartInfo()
                {
                    FileName = Path.Combine(Environment.CurrentDirectory, "Addins", "ScreenCapture.exe"),
                    Arguments = args
                });
                process.WaitForExit();

            }
            catch (Exception ex)
            {
                MessageBox.Show("Unable to capture screen shot.\r\n" + ex.Message, mmApp.ApplicationName,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!File.Exists(resultFilePath))
                return;

            // read the location for the captured image so we can embed a link to it
            string capturedFile = File.ReadAllText(resultFilePath);

            if (string.IsNullOrEmpty(capturedFile) || capturedFile.StartsWith("Cancelled") ||
                !File.Exists(capturedFile))
                return;

            capturedFile = FileUtils.GetRelativePath(capturedFile, imageFolder);
            string relPath = capturedFile.Replace("\\", "/");
            if (relPath.StartsWith(".."))
                relPath = capturedFile;

            if (relPath.Contains(":\\")) // full path
                relPath = "file:///" + relPath.Replace("\\", "/");


            string replaceText = "![](" + relPath + ")";

            // Push the new text into the Editor's Selection
            SetSelection(replaceText);
        }
#endif

        public override void OnExecuteConfiguration(object sender)
        {
            var configForm = new ScreenCaptureConfigurationForm()
            {
                Owner = this.Model.Window
            };            
            configForm.Show();
        }

        public override bool OnCanExecute(object sender)
        {            
            return true;
        }
    }
}
