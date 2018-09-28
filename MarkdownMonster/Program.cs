using System;
using System.Diagnostics;
using MarkdownMonster;

class StartUp
{
    [STAThread]
    public static void Main()
    {

        try
        {            
            App app = new App();
            app.InitializeComponent();
            app.Run();

            Process.GetCurrentProcess().Kill(); //Must add this line after app.Run!!!
        }
        catch { }
        
    }
}
