using System;

using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace ScreenCaptureAddin
{
    public class ScreenCapture
    {
        public static Image CaptureWindow(IntPtr handle)
        {
            var rect = GetWindowRectangle(handle);
            if (rect.Width == 0 || rect.Height == 0)
                return null;

            return CaptureWindow(rect);
        }

     

        /// <summary>
        /// Creates an Image object containing a screen shot of a specific window
        /// </summary>
        /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>
        /// <returns></returns>
        public static Image CaptureWindow(Rectangle rect)
        {
            // allow window animations to complete
            Application.DoEvents();
            Thread.Sleep(400);

            var result = new Bitmap(rect.Width, rect.Height);

            Application.DoEvents();

            using (var graphics = Graphics.FromImage(result))
            {
                int strip = 0; //(int) FrameStyle.Thick;

                graphics.CopyFromScreen(
                    new Point(rect.Left + strip, rect.Top + strip),
                    Point.Empty,
                    new Size()
                    {
                        Width = rect.Width - (2 * strip),
                        Height = rect.Height - (2 * strip)
                    });
            }

            return result;
        }




        public static Bitmap CaptureWindowBitmap(IntPtr handle)
        {
            using (var img = CaptureWindow(handle)) { 
                if (img == null)
                    return null;

                return new Bitmap(img);
            }            
        }


        public static BitmapSource ImageToBitmapSource(Image img)
        {
            using (var bmp = new Bitmap(img))
            {
                var hBitmap = bmp.GetHbitmap();
                var imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero,
                    System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(hBitmap);
                return imageSource;
            }
        }

        public static BitmapSource BitmapToBitmapSource(Bitmap bmp)
        {                
                var hBitmap = bmp.GetHbitmap();
                var imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, System.Windows.Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                DeleteObject(hBitmap);
                return imageSource;           
        }

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeleteObject(IntPtr hObject);


        /// <summary>
        /// Figures out accurate Windows bounds based on a given Window handle        
        /// </summary>
        /// <param name="handle">Windows hownd</param>
        /// <returns></returns>
        public static  Rectangle GetWindowRectangle(IntPtr handle)
        {
            Rectangle rected = Rectangle.Empty;

            Rect rect = new Rect();
            if (Environment.OSVersion.Version.Major < 6)
            {
                GetWindowRect(handle, out rect);
                rected = rect.ToRectangle();
            }
            else
            {
                int size = Marshal.SizeOf(typeof(Rect));
                DwmGetWindowAttribute(handle, (int)DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS, out rect, size);
                rected = rect.ToRectangle();

                // allow returning of desktop and aero windows
                if (rected.Width == 0)
                {
                    GetWindowRect(handle, out rect);
                    rected = rect.ToRectangle();
                }
            }

            return rected;
        }

        

        #region Api Definitions

        [Serializable, StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public Rectangle ToRectangle()
            {
                return Rectangle.FromLTRB(Left, Top, Right, Bottom);
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr handle, out Rect Rect);

        [DllImport("dwmapi.dll")]
        static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out Rect pvAttribute, int cbAttribute);

        [Flags]
        public enum DwmWindowAttribute : uint
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        }

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(Point point);


        /// <summary>
        /// Show or hide a window based on a Window handle
        /// </summary>
        /// <param name="handle">Window handle</param>
        /// <param name="windowMode">0 - hide 5 - show  6 - minimize</param>
        /// <returns></returns>
        [DllImport("user32.dll")]
        public static extern int ShowWindow(IntPtr handle, int windowMode);  // 0 hide - 6 minimize - 5 show

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();


        [StructLayout(LayoutKind.Sequential)]
        internal struct CURSORINFO
        {
            public Int32 cbSize;
            public Int32 flags;
            public IntPtr hCursor;
            public POINTAPI ptScreenPos;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct POINTAPI
        {
            public int x;
            public int y;
        }

        [DllImport("user32.dll")]
        internal static extern bool GetCursorInfo(out CURSORINFO pci);

        [DllImport("user32.dll")]
        static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);

        const Int32 CURSOR_SHOWING = 0x00000001;

        internal static CURSORINFO GetMousePointerInfo()
        {
            CURSORINFO pci;
            pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));
            GetCursorInfo(out pci);
            return pci;
        }

        internal static void DrawMousePointer(CURSORINFO pci, Bitmap bmp)
        {
            if (pci.hCursor == IntPtr.Zero || bmp == null)
                return;
            
            pci.cbSize = Marshal.SizeOf(typeof(CURSORINFO));

            using (Graphics g = Graphics.FromImage(bmp))
            {                
                if (pci.flags == CURSOR_SHOWING)
                {
                    DrawIcon(g.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                    g.ReleaseHdc();
                }             
            }
        }

        public static Bitmap CaptureScreen(bool CaptureMouse)
        {
            Bitmap result = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format24bppRgb);

            try
            {
                using (Graphics g = Graphics.FromImage(result))
                {
                    g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size, CopyPixelOperation.SourceCopy);

                    if (CaptureMouse)
                    {
                        CURSORINFO pci;
                        pci.cbSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(CURSORINFO));

                        if (GetCursorInfo(out pci))
                        {
                            if (pci.flags == CURSOR_SHOWING)
                            {
                                DrawIcon(g.GetHdc(), pci.ptScreenPos.x, pci.ptScreenPos.y, pci.hCursor);
                                g.ReleaseHdc();
                            }
                        }
                    }
                }
            }
            catch
            {
                result = null;
            }

            return result;
        }


        #endregion
    }


    public class WindowInfo
    {
        public IntPtr Handle;
        //public string ClassName;
        //public string Text;
        public Rectangle Rect;

        public WindowInfo(IntPtr Handle)
        {
            this.Handle = Handle;
            //ClassName = GetWindowClassName(Handle);
            //Text = GetWindowText(Handle);
            Rect = ScreenCapture.GetWindowRectangle(Handle);
        }
    }
}
