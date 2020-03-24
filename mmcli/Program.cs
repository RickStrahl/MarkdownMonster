using System;

namespace mmcli
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var processor = new CommandLineProcessor();
            processor.HandleCommandLineArguments();
        }
    }
}
