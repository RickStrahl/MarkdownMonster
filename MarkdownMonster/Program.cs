using System;
using System.Diagnostics;
using System.Reflection;
using MarkdownMonster;

class StartUp
{
    [STAThread]
    public static void Main()
    {

#if true
         App app = new App();
         app.InitializeComponent();
         app.Run();

         Process.GetCurrentProcess().Kill(); //Must add this line after app.Run!!!
#else

        // this code ends up taking over all errors
        // better to let the dispatcher handle this
        App app = null;
        try
        {
            app = new App();
            app.InitializeComponent();
            app.Run();
            Process.GetCurrentProcess().Kill(); //Must add this line after app.Run!!!
        }
        catch (Exception e)
        {
            if (!mmApp.HandleApplicationException(e as Exception, ApplicationErrorModes.AppRoot))
            {
                Environment.Exit(1);
            }

            app?.Shutdown();
            app = null;

            // restart with same arguments
            var process = Process.GetCurrentProcess();
            Process.Start(Assembly.GetExecutingAssembly().Location);
            process.Kill();

            //Environment.Exit(1);
        }


#endif
    }


}
