using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MarkdownMonster
{
    //public class Program
    //{
    //    public static Mutex Mutex;


    //    [STAThread]
    //    public static void Main()
    //    {
    //        if (mmApp.Configuration.UseSingleWindow)
    //        {
    //            bool isOnlyInstance = false;
    //            Mutex = new Mutex(true, @"MarkdownMonster", out isOnlyInstance);
    //            if (!isOnlyInstance)
    //            {
    //                string filesToOpen = " ";
    //                var args = Environment.GetCommandLineArgs();
    //                if (args != null && args.Length > 1)
    //                {
    //                    StringBuilder sb = new StringBuilder();
    //                    for (int i = 1; i < args.Length; i++)
    //                    {
    //                        sb.AppendLine(args[i]);
    //                    }
    //                    filesToOpen = sb.ToString();
    //                }

    //                //File.WriteAllText(mmApp.Configuration.FileWatcherOpenFilePath, filesToOpen);

    //                var manager = new NamedPipeManager("MarkdownMonster");
    //                manager.Write(filesToOpen);

    //                Mutex.Dispose();
    //                mmApp.Configuration = null;

    //                GC.SuppressFinalize(manager);

    //                Environment.Exit(0);
    //            }
    //        }

    //        var application = new App();
    //        application.InitializeComponent();
    //        application.Run();
    //    }
    //}
}
