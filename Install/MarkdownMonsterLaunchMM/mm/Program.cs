using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace mm
{
    class Program
    {
        static void Main(string[] args)
        {
            var dir = System.Environment.CurrentDirectory;
            
            var si = new ProcessStartInfo()
            {
                 FileName = "MarkdownMonster.exe",                 
                 WorkingDirectory = dir,
                 WindowStyle = ProcessWindowStyle.Hidden,
                 CreateNoWindow = true                 
            };

            if (args != null)
            {
                string argString = "";
                foreach (var arg in args)
                {
                    argString += $"\"{arg}\" ";                    
                }
                si.Arguments = argString;
            }

            Process.Start(si);
        }
    }
}
