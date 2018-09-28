using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace SnagItAddin
{
    public class WinFormOverlay : IDisposable
    {
        public Form f;

        public WinFormOverlay()
        {
        }

        public void Start()
        {
            var t = new Thread(StartProc)
            {
                Name = "SnagItCaptureThread",
                IsBackground = true
            };
            t.Start();
        }

        public void StartProc(object parm)
        {
            if (f == null)
            {
                f = new Form();

                f.TopLevel = true;
                f.TopMost = true;

                f.BackColor = System.Drawing.Color.Violet;
                //f.TransparencyKey = System.Drawing.Color.Violet;

                f.FormBorderStyle = FormBorderStyle.Sizable;                

                //System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.Run(f);

                f.Show();
            }
        }

        public void SetLocation(int left, int top, int width, int height, string text = null)
        {
            f?.Invoke(new Action(() =>
            {
                f.Left = left;
                f.Top = top;
                f.Width = width;
                f.Height = height;

                f.Text = text;                

                Debug.WriteLine("Win Form: " + left + "x" + top);
            }));
        }

        public void Stop()
        {
            Dispose();
        }

        public void Dispose()
        {
            f?.Invoke(new Action(() =>
            {
                f?.Close();
                f = null;

                
            }));

            System.Windows.Forms.Application.Exit();            
        }
    }
}
