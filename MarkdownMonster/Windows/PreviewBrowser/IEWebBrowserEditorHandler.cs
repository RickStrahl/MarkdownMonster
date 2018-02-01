using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Navigation;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
using WebBrowser = System.Windows.Controls.WebBrowser;

namespace MarkdownMonster.Windows.PreviewBrowser
{
    public class IEWebBrowserEditorHandler    : Native.IDocHostUIHandler
    { 
        private const uint E_NOTIMPL = 0x80004001;
        private const uint S_OK = 0;
        private const uint S_FALSE = 1;

        public IEWebBrowserEditorHandler(WebBrowser browser)
        {
            if (browser == null)
                throw new ArgumentNullException("browser");

            Browser = browser;
            browser.LoadCompleted += OnLoadCompleted;
            browser.Navigated += OnNavigated;

            IsWebBrowserContextMenuEnabled = false;
            ScriptErrorsSuppressed = true;

            Flags |= HostUIFlags.DPI_AWARE | HostUIFlags.ENABLE_REDIRECT_NOTIFICATION | HostUIFlags.FLAT_SCROLLBAR |
                     HostUIFlags.DISABLE_SCRIPT_INACTIVE;

        }

        public WebBrowser Browser { get; private set; }
        public HostUIFlags Flags { get; set; }
        public bool IsWebBrowserContextMenuEnabled { get; set; }
        public bool ScriptErrorsSuppressed { get; set; }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            SetSilent(Browser, ScriptErrorsSuppressed);
        }

        private void OnLoadCompleted(object sender, NavigationEventArgs e)
        {
            Native.ICustomDoc doc = Browser.Document as Native.ICustomDoc;
            doc?.SetUIHandler(this);
        }

        uint Native.IDocHostUIHandler.ShowContextMenu(int dwID, Native.POINT pt, object pcmdtReserved,
            object pdispReserved)
        {
            return IsWebBrowserContextMenuEnabled ? S_FALSE : S_OK;
        }

        uint Native.IDocHostUIHandler.GetHostInfo(ref Native.DOCHOSTUIINFO info)
        {
            info.dwFlags = (int) Flags;
            info.dwDoubleClick = 0;
            return S_OK;
        }

        uint Native.IDocHostUIHandler.ShowUI(int dwID, object activeObject, object commandTarget, object frame,
            object doc)
        {
            return E_NOTIMPL;
        }

        uint Native.IDocHostUIHandler.HideUI()
        {
            return E_NOTIMPL;
        }

        uint Native.IDocHostUIHandler.UpdateUI()
        {
            return E_NOTIMPL;
        }

        uint Native.IDocHostUIHandler.EnableModeless(bool fEnable)
        {
            return E_NOTIMPL;
        }

        uint Native.IDocHostUIHandler.OnDocWindowActivate(bool fActivate)
        {
            return E_NOTIMPL;
        }

        uint Native.IDocHostUIHandler.OnFrameWindowActivate(bool fActivate)
        {
            return E_NOTIMPL;
        }

        uint Native.IDocHostUIHandler.ResizeBorder(Native.COMRECT rect, object doc, bool fFrameWindow)
        {
            return E_NOTIMPL;
        }

        uint Native.IDocHostUIHandler.TranslateAccelerator(ref Message msg, ref Guid group, int nCmdID)
        {
            return S_FALSE;
        }

        uint Native.IDocHostUIHandler.GetOptionKeyPath(string[] pbstrKey, int dw)
        {
            return E_NOTIMPL;
        }

        uint Native.IDocHostUIHandler.GetDropTarget(object pDropTarget, out object ppDropTarget)
        {
            ppDropTarget = null;
            return E_NOTIMPL;
        }

        uint Native.IDocHostUIHandler.GetExternal(out object ppDispatch)
        {
            ppDispatch = Browser.ObjectForScripting;
            return S_OK;
        }

        uint Native.IDocHostUIHandler.TranslateUrl(int dwTranslate, string strURLIn, out string pstrURLOut)
        {
            pstrURLOut = null;
            return E_NOTIMPL;
        }

        uint Native.IDocHostUIHandler.FilterDataObject(IDataObject pDO, out IDataObject ppDORet)
        {
            ppDORet = null;
            return E_NOTIMPL;
        }

        public static void SetSilent(WebBrowser browser, bool silent)
        {
            Native.IOleServiceProvider sp = browser.Document as Native.IOleServiceProvider;
            if (sp != null)
            {
                Guid IID_IWebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
                Guid IID_IWebBrowser2 = new Guid("D30C1661-CDAF-11d0-8A3E-00C04FC9E26E");

                object webBrowser;
                sp.QueryService(ref IID_IWebBrowserApp, ref IID_IWebBrowser2, out webBrowser);
                if (webBrowser != null)
                {
                    webBrowser.GetType()
                        .InvokeMember("Silent",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.PutDispProperty, null,
                            webBrowser, new object[] {silent});
                }
            }
        }
    }

    internal static class Native
    {
        [ComImport, Guid("BD3F23C0-D43E-11CF-893B-00AA00BDCE1A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IDocHostUIHandler
        {
            [PreserveSig]
            uint ShowContextMenu(int dwID, POINT pt, [MarshalAs(UnmanagedType.Interface)] object pcmdtReserved,
                [MarshalAs(UnmanagedType.Interface)] object pdispReserved);

            [PreserveSig]
            uint GetHostInfo(ref DOCHOSTUIINFO info);

            [PreserveSig]
            uint ShowUI(int dwID, [MarshalAs(UnmanagedType.Interface)] object activeObject,
                [MarshalAs(UnmanagedType.Interface)] object commandTarget,
                [MarshalAs(UnmanagedType.Interface)] object frame, [MarshalAs(UnmanagedType.Interface)] object doc);

            [PreserveSig]
            uint HideUI();

            [PreserveSig]
            uint UpdateUI();

            [PreserveSig]
            uint EnableModeless(bool fEnable);

            [PreserveSig]
            uint OnDocWindowActivate(bool fActivate);

            [PreserveSig]
            uint OnFrameWindowActivate(bool fActivate);

            [PreserveSig]
            uint ResizeBorder(COMRECT rect, [MarshalAs(UnmanagedType.Interface)] object doc, bool fFrameWindow);

            [PreserveSig]
            uint TranslateAccelerator(ref Message msg, ref Guid group, int nCmdID);

            [PreserveSig]
            uint GetOptionKeyPath([Out, MarshalAs(UnmanagedType.LPArray)] string[] pbstrKey, int dw);

            [PreserveSig]
            uint GetDropTarget([In, MarshalAs(UnmanagedType.Interface)] object pDropTarget,
                [MarshalAs(UnmanagedType.Interface)] out object ppDropTarget);

            [PreserveSig]
            uint GetExternal([MarshalAs(UnmanagedType.IDispatch)] out object ppDispatch);

            [PreserveSig]
            uint TranslateUrl(int dwTranslate, [MarshalAs(UnmanagedType.LPWStr)] string strURLIn,
                [MarshalAs(UnmanagedType.LPWStr)] out string pstrURLOut);

            [PreserveSig]
            uint FilterDataObject(IDataObject pDO, out IDataObject ppDORet);
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct DOCHOSTUIINFO
        {
            public int cbSize;
            public int dwFlags;
            public int dwDoubleClick;
            public IntPtr dwReserved1;
            public IntPtr dwReserved2;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct COMRECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class POINT
        {
            public int x;
            public int y;
        }

        [ComImport, Guid("3050F3F0-98B5-11CF-BB82-00AA00BDCE0B"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ICustomDoc
        {
            [PreserveSig]
            int SetUIHandler(IDocHostUIHandler pUIHandler);
        }

        [ComImport, Guid("6D5140C1-7436-11CE-8034-00AA006009FA"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IOleServiceProvider
        {
            [PreserveSig]
            uint QueryService([In] ref Guid guidService, [In] ref Guid riid,
                [MarshalAs(UnmanagedType.IDispatch)] out object ppvObject);
        }
    }

    [Flags]
    public enum HostUIFlags
    {
        DIALOG = 0x00000001,
        DISABLE_HELP_MENU = 0x00000002,
        NO3DBORDER = 0x00000004,
        SCROLL_NO = 0x00000008,
        DISABLE_SCRIPT_INACTIVE = 0x00000010,
        OPENNEWWIN = 0x00000020,
        DISABLE_OFFSCREEN = 0x00000040,
        FLAT_SCROLLBAR = 0x00000080,
        DIV_BLOCKDEFAULT = 0x00000100,
        ACTIVATE_CLIENTHIT_ONLY = 0x00000200,
        OVERRIDEBEHAVIORFACTORY = 0x00000400,
        CODEPAGELINKEDFONTS = 0x00000800,
        URL_ENCODING_DISABLE_UTF8 = 0x00001000,
        URL_ENCODING_ENABLE_UTF8 = 0x00002000,
        ENABLE_FORMS_AUTOCOMPLETE = 0x00004000,
        ENABLE_INPLACE_NAVIGATION = 0x00010000,
        IME_ENABLE_RECONVERSION = 0x00020000,
        THEME = 0x00040000,
        NOTHEME = 0x00080000,
        NOPICS = 0x00100000,
        NO3DOUTERBORDER = 0x00200000,
        DISABLE_EDIT_NS_FIXUP = 0x00400000,
        LOCAL_MACHINE_ACCESS_CHECK = 0x00800000,
        DISABLE_UNTRUSTEDPROTOCOL = 0x01000000,
        HOST_NAVIGATES = 0x02000000,
        ENABLE_REDIRECT_NOTIFICATION = 0x04000000,
        USE_WINDOWLESS_SELECTCONTROL = 0x08000000,
        USE_WINDOWED_SELECTCONTROL = 0x10000000,
        ENABLE_ACTIVEX_INACTIVATE_MODE = 0x20000000,
        DPI_AWARE = 0x40000000
    }



}
