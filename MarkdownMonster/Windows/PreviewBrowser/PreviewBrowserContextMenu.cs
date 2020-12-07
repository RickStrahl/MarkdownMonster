using System;
using System.Drawing;
using System.IO;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using MarkdownMonster.Windows.DocumentOutlineSidebar;
using Westwind.Utilities;

namespace MarkdownMonster.Windows.PreviewBrowser
{
    public class PreviewBrowserContextMenu
    {
        public static event EventHandler<ContextMenu> ContextMenuOpening;


        /// <summary>
        /// Creates a context menu.
        /// </summary>
        /// <param name="parms"></param>
        /// <param name="model"></param>
        /// <param name="webBrowser"></param>
        public void ShowContextMenu(PositionAndDocumentType parms, AppModel model, FrameworkElement webBrowser)
        {
            if (webBrowser == null)
                return;

            var ctm = new ContextMenu();
            MenuItem mi;

            // Image Selected
            if (!string.IsNullOrEmpty(parms.Src))
            {
                mi = new MenuItem()
                {
                    Header = "Copy Image to Clipboard"
                };
                mi.Click += (o, args) =>
                {
                    string image = null;
                    bool deleteFile = false;

                    if (parms.Src.StartsWith("https://") || parms.Src.StartsWith("http://"))
                    {
                        image = HttpUtils.DownloadImageToFile(parms.Src);
                        if (string.IsNullOrEmpty(image))
                        {
                            model.Window.ShowStatusError("Unable to copy image from URL to clipboard: " + parms.Src);
                            return;
                        }
                        deleteFile = true;
                    }
                    else
                    {
                        try
                        {
                            image = new Uri(parms.Src).LocalPath;
                        }
                        catch
                        {
                            image = FileUtils.NormalizePath(parms.Src);
                        }

                        image = mmFileUtils.NormalizeFilenameWithBasePath(image,
                            Path.GetDirectoryName(model.ActiveDocument.Filename));
                    }

                    try
                    {
                        BitmapSource bmpSrc;
                        using (var bmp = new Bitmap(image))
                        {
                            bmpSrc = WindowUtilities.BitmapToBitmapSource(bmp);
                            Clipboard.SetImage(bmpSrc);
                        }

                        model.Window.ShowStatusSuccess("Image copied to clipboard.");
                    }
                    catch (Exception ex)
                    {
                        model.Window.ShowStatusError("Couldn't copy image to clipboard: " + ex.Message);
                    }
                    finally
                    {
                        if (deleteFile && File.Exists(image))
                            File.Delete(image);
                    }
                };
                ctm.Items.Add(mi);

                mi = new MenuItem()
                {
                    Header = "Edit Image in Image editor"
                };
                mi.Click += (o, args) =>
                {
                    string image = null;
                    if (parms.Src.StartsWith("https://") || parms.Src.StartsWith("http://"))
                    {
                        image = HttpUtils.DownloadImageToFile(parms.Src);
                        if (string.IsNullOrEmpty(image))
                        {
                            model.Window.ShowStatusError("Unable to copy image from URL to clipboard: " + parms.Src);
                            return;
                        }
                    }
                    else
                    {
                        try
                        {
                            image = new Uri(parms.Src).LocalPath;
                        }
                        catch
                        {
                            image = FileUtils.NormalizePath(parms.Src);
                        }
                        image = mmFileUtils.NormalizeFilenameWithBasePath(image, Path.GetDirectoryName(model.ActiveDocument.Filename));
                    }
         
                    mmFileUtils.OpenImageInImageEditor(image);
                };
                ctm.Items.Add(mi);

                ctm.Items.Add(new Separator());
            }

            
            // HREF link selected
            if (!string.IsNullOrEmpty(parms.Href))
            {
                // Navigate relative hash links in the document
                if (parms.Href.StartsWith("#") && parms.Href.Length > 1)
                {
                    var docModel = new DocumentOutlineModel();
                    int lineNo = docModel.FindHeaderHeadline(model.ActiveEditor?.GetMarkdown(), parms.Href?.Substring(1));
                    if (lineNo > -1)
                    {
                        mi = new MenuItem()
                        {
                            Header = "Jump to: " + parms.Href, CommandParameter = parms.Href.Substring(1)
                        };
                        mi.Click += (s, e) =>
                        {
                            var mitem = s as MenuItem;

                            var anchor = mitem.CommandParameter as string;
                            if (string.IsNullOrEmpty(anchor))
                                return;

                            docModel = new DocumentOutlineModel();
                            lineNo = docModel.FindHeaderHeadline(model.ActiveEditor?.GetMarkdown(), anchor);

                            if (lineNo != -1)
                                model.ActiveEditor.GotoLine(lineNo);
                        };
                        ctm.Items.Add(mi);

                        ctm.Items.Add(new Separator());
                    }
                }
            }

            // ID to clipboard
            if (!string.IsNullOrEmpty(parms.Id))
            {
                mi = new MenuItem()
                {
                    Header = "Copy Id to Clipboard: #" + parms.Id,

                };
                mi.Click += (s, e) =>
                {
                    ClipboardHelper.SetText("#" + parms.Id);
                    model.Window.ShowStatusSuccess("'#" + parms.Id + "' copied to the clipboard.");
                };
                ctm.Items.Add(mi);

                ctm.Items.Add(new Separator());
            }


            mi = new MenuItem()
            {
                Header = "View in Web _Browser",
                Command = model.Commands.ViewInExternalBrowserCommand,
                InputGestureText = model.Commands.ViewInExternalBrowserCommand.KeyboardShortcut
            };
            ctm.Items.Add(mi);

            mi = new MenuItem()
            {
                Header = "Refresh _Browser",
                Command = model.Commands.RefreshPreviewCommand,
                InputGestureText = "F5"
            };
            ctm.Items.Add(mi);

            mi = new MenuItem()
            {
                Header = "View Html _Source",
                Command = model.Commands.ViewHtmlSourceCommand
            };
            ctm.Items.Add(mi);

            ctm.Items.Add(new Separator());


            mi = new MenuItem()
            {
                Header = "Save As _Html",
                Command = model.Commands.SaveAsHtmlCommand,
            };
            ctm.Items.Add(mi);

            mi = new MenuItem()
            {
                Header = "Save As _PDF",
                Command = model.Commands.GeneratePdfCommand,
            };
            ctm.Items.Add(mi);

            mi = new MenuItem()
            {
                Header = "P_rint...",
                Command = model.Commands.PrintPreviewCommand,
            };
            ctm.Items.Add(mi);

            ctm.Items.Add(new Separator());

            mi = new MenuItem()
            {
                Header = "Edit Preview _Theme",
                Command = model.Commands.EditPreviewThemeCommand,
            };
            ctm.Items.Add(mi);

            mi = new MenuItem()
            {
                Header = "Configure Preview Syncing",
                Command = model.Commands.PreviewSyncModeCommand,
            };
            ctm.Items.Add(mi);

            ctm.Items.Add(new Separator());


            mi = new MenuItem()
            {
                Header = "Toggle Preview Window",
                Command = model.Commands.TogglePreviewBrowserCommand,
                CommandParameter = "Toggle",
                InputGestureText = model.Commands.TogglePreviewBrowserCommand.KeyboardShortcut,
                IsCheckable = true,
                IsChecked = model.IsPreviewBrowserVisible
            };
            ctm.Items.Add(mi);

            if (model.Configuration.System.ShowPreviewDeveloperTools)
            {
                ctm.Items.Add(new Separator());
                mi = new MenuItem()
                {
                    Header = "Show Browser Developer Tools"
                };
                mi.Click += (s, a) => model.Window.PreviewBrowser.ShowDeveloperTools();
                ctm.Items.Add(mi);
            }

            webBrowser.ContextMenu = ctm;
            ContextMenuOpening?.Invoke(this, ctm);

            ctm.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            ctm.PlacementTarget = webBrowser;
            ctm.IsOpen = true;
        }
    }

    public class PositionAndDocumentType
    {

        public PositionAndDocumentType(object parms = null)
        {
            if (parms == null)
                return;

            Left = (int)ReflectionUtils.GetPropertyCom(parms, "Left");
            Top = (int)ReflectionUtils.GetPropertyCom(parms, "Top");
            Id = ReflectionUtils.GetPropertyCom(parms, "Id") as string;
            Src = ReflectionUtils.GetPropertyCom(parms, "Src") as string;
            Href = ReflectionUtils.GetPropertyCom(parms, "Href") as string;
        }


        public int Left { get; set; }
        public int Top { get; set; }
        public string Id { get; set; }
        public string Type { get; set; }
        public string Src { get; set; }
        public string Href { get; set; }
    }
}
