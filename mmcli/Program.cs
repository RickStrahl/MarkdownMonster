using System;

namespace mmcli
{
    class Program
    {
        static void Main(string[] args)
        {
            var processor = new CommandLineProcessor();
            processor.HandleCommandLineArguments();
        }
    }
}
