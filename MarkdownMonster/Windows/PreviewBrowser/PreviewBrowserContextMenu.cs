using System.IO;
using System.Web;
using System.Windows.Controls;
using MarkdownMonster.Windows.DocumentOutlineSidebar;
using Westwind.Utilities;

namespace MarkdownMonster.Windows.PreviewBrowser
{
    public class PreviewBrowserContextMenu
    {
        /// <summary>
        /// Creates a context menu. 
        /// </summary>
        /// <param name="parms"></param>
        /// <param name="model"></param>
        /// <param name="webBrowser"></param>
        public void ShowContextMenu(PositionAndDocumentType parms, AppModel model, WebBrowser webBrowser)
        {
            var ctm = new ContextMenu();

            var mi = new MenuItem()
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
                Header = "Save As _Pdf",
                Command = model.Commands.GeneratePdfCommand,
            };
            ctm.Items.Add(mi);

            mi = new MenuItem()
            {
                Header = "P_rint...",
                Command = model.Commands.PrintPreviewCommand,
            };
            ctm.Items.Add(mi);

            bool separatorAdded = false;

            if (!string.IsNullOrEmpty(parms.Id))
            {
                ctm.Items.Add(new Separator());
                separatorAdded = true;

                mi = new MenuItem()
                {
                    Header = "Copy Id to Clipboard: #" + parms.Id,

                };
                mi.Click += (s, e) =>
                {
                    ClipboardHelper.SetText("#" + parms.Id);
                };
                ctm.Items.Add(mi);
            }

            if (!string.IsNullOrEmpty(parms.Src))
            {
                if (!separatorAdded)
                {
                    ctm.Items.Add(new Separator());
                    separatorAdded = true;
                }
                
                mi = new MenuItem()
                {
                    Header = "Edit Image in Image editor"
                };
                mi.Click += (o, args) =>
                {
                    var image = HttpUtility.UrlDecode(parms.Src.Replace("file:///", ""));
                    image = mmFileUtils.NormalizeFilenameWithBasePath(image,
                        Path.GetDirectoryName(model.ActiveDocument.Filename));
                    mmFileUtils.OpenImageInImageEditor(image);
                };
                ctm.Items.Add(mi);
            }

            if (!string.IsNullOrEmpty(parms.Href))
            {
                // Navigate relative hash links in the document
                if (parms.Href.StartsWith("#") && parms.Href.Length > 1)
                {
                    if (!separatorAdded)
                    {
                        ctm.Items.Add(new Separator());
                        separatorAdded = true;
                    }

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
                    }
                }
            }


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
                IsCheckable = true,
                InputGestureText = model.Commands.TogglePreviewBrowserCommand.KeyboardShortcut,
                IsChecked = model.IsPreviewBrowserVisible
            };
            ctm.Items.Add(mi);

            webBrowser.ContextMenu = ctm;
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
