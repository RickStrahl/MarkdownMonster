//if false
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using MahApps.Metro.Controls;
using MarkdownMonster;
using Westwind.RazorHosting;
using Westwind.Utilities;
//using Microsoft.CodeAnalysis.CSharp.Scripting;
//using Microsoft.CodeAnalysis.Scripting;
using Westwind.wwScripting;

namespace SnippetsAddin
{

    /// <summary>
    /// A minimal Script Parser class that uses {{ C# Expressions }} to evaluate
    /// string based templates.
    /// </summary>
    /// <example>
    /// string script = @"Hi {{Name}}! Time is {{DateTime.sNow.ToString(""MMM dd, yyyy HH:mm:ss"")}}...";    
    /// var parser = new ScriptParser();
    /// string result = await parser.EvaluateScriptAsync(script, new Globals { Name = "Rick" });
    /// </example>
    public class ScriptParser
    {        
        // Additional namespaces to add to the script
        public List<string> Namespaces = new List<string>();

        /// <summary>
        ///  Additional references to add beyond MsCoreLib and System 
        /// Pass in a type from a give assembly
        /// </summary>        
        public List<string> References = new List<string>();

        //wwScriptingRoslyn ScriptCompiler = new wwScriptingRoslyn();

        public string ErrorMessage { get; set; }



        /// <summary>
        /// Evaluates the embedded script parsing out {{ C# Expression }} 
        /// blocks and evaluating the expressions and embedding the string
        /// output into the result string.
        /// 
        /// 
        /// </summary>
        /// <param name="snippet">The snippet template to expand</param>
        /// <param name="model">Optional model data accessible in Expressions as `Model`</param>
        /// <returns></returns>
        public string EvaluateScript(string snippet, object model = null)
        {
#if DEBUG
            var sw = new Stopwatch();
            sw.Start();
#endif
            var tokens = TokenizeString(ref snippet, "{{", "}}");

            snippet = snippet.Replace("\"","\"\"");

            snippet = DetokenizeString(snippet,tokens);
                                
            snippet = snippet.Replace("{{", "\" + ").Replace("}}", " + @\"");
            snippet = "@\"" + snippet + "\"";            

            
            string code = "dynamic Model = Parameters[0];\r\n" +                          
                          "return " + snippet + ";";

            var scriptCompiler = new wwScriptingRoslyn();
            string result = scriptCompiler.ExecuteCode(code,model) as string;
            
            if (result == null)
                ErrorMessage = scriptCompiler.ErrorMessage;
            else
                ErrorMessage = null;

#if DEBUG
            sw.Stop();
            Debug.WriteLine("ScriptParser Code: \r\n" + code);
            Debug.WriteLine("Snippet EvaluateScript Execution Time: " + sw.ElapsedMilliseconds + "ms");
#endif

            return result;
        }
       

        public static RazorStringHostContainer RazorHost
        {
            get
            {
                if (_razorHost == null)
                {
                    _razorHost = new RazorStringHostContainer();
                    _razorHost.UseAppDomain = false;
                    
                    _razorHost.CodeProvider = new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider();

                    RazorHost.AddAssemblyFromType(typeof(MainWindow));   // MarkdownMonster.exe
                    RazorHost.AddAssemblyFromType(typeof(StringUtils));  // Westwind.Utilities
                    RazorHost.AddAssemblyFromType(typeof(DbConnection));  // System.Data
                    RazorHost.AddAssemblyFromType(typeof(Graphics));  // System.Data
                    
                    RazorHost.ReferencedNamespaces.Add("MarkdownMonster");
                    RazorHost.ReferencedNamespaces.Add("Westwind.Utilities");
                    RazorHost.ReferencedNamespaces.Add("System.IO");
                    RazorHost.ReferencedNamespaces.Add("System.Text");
                    RazorHost.ReferencedNamespaces.Add("System.Drawing");
                    RazorHost.ReferencedNamespaces.Add("System.Data");
                    RazorHost.ReferencedNamespaces.Add("System.Data.SqlClient");                    

                    _razorHost.Start();
                                    
                }
                return _razorHost;
            }

        }
        private static RazorStringHostContainer _razorHost;


        /// <summary>
        /// Evaluate script using ASP.NET MVC Razor style scripts.
        /// </summary>
        /// <param name="snippet"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public string EvaluateRazorScript(string snippet, object state = null)
        {
            string result = null;

            var snippetLines = StringUtils.GetLines(snippet);
            var sb = new StringBuilder();
            foreach (var line in snippetLines)
            {
                if (line.Trim().Contains("@reference "))
                {
                    string assemblyName = line.Replace("@reference ", "").Trim();

                    if (assemblyName.Contains("\\") || assemblyName.Contains("/") )
                    {
                        ErrorMessage = "Assemblies loaded from external folders are not allowed: " + assemblyName +
                                       "\r\n\r\n" +
                                       "Referenced assemblies can only be loaded out of the Markdown Monster startup folder and you have to explicitly add any assemblies you want to reference there.";
                        return null;
                    }

                    var fullAssemblyName = Path.GetFullPath(assemblyName);
                    if (File.Exists(fullAssemblyName))
                        assemblyName = fullAssemblyName;

                    // Add to Engine since host is already instantiated
                    RazorHost.Engine.AddAssembly(assemblyName);
                    continue;
                }
                if (line.Trim().Contains("@using "))
                {
                    string ns = line.Replace("@using ", "").Trim();

                    // Add to Engine since host is already instantiated
                    RazorHost.Engine.AddNamespace(ns);                                            
                    continue;
                }

                sb.AppendLine(line);
            }

            snippet = sb.ToString();

            // Render the actual template and pass the model
            result = RazorHost.RenderTemplate(snippet,state);
            if (result == null)
                ErrorMessage = RazorHost.ErrorMessage + "\r\n" + RazorHost.Engine?.LastGeneratedCode;

            return result;
        }


        /// <summary>
        /// Tokenizes a string based on a start and end string. Replaces the values with a token
        /// value (#@#1#@# for example).
        /// 
        /// You can use Detokenize to get the original values back
        /// </summary>
        /// <param name="text"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="replaceDelimiter"></param>
        /// <returns></returns>
        public List<string> TokenizeString(ref string text, string start, string end, string replaceDelimiter = "#@#")
        {
            var strings = new List<string>();

            var regex = new Regex("{{.*?}}");

            var matches = regex.Matches(text);

            int i = 0;
            foreach (Match match in matches)
            {
                regex = new Regex(Regex.Escape(match.Value));
                text = regex.Replace(text, $"{replaceDelimiter}{i}{replaceDelimiter}",1);
                strings.Add(match.Value);
                i++;
            }
            
            return strings;
        }


        /// <summary>
        /// Detokenizes a string tokenized with TokenizeString. Requires the collection created
        /// by detokenization
        /// </summary>
        /// <param name="text"></param>
        /// <param name="tokens"></param>
        /// <param name="replaceDelimiter"></param>
        /// <returns></returns>
        public string DetokenizeString(string text, List<string> tokens, string replaceDelimiter = "#@#")
        {
            int i = 0;
            foreach (string token in tokens)
            {
                text = text.Replace($"{replaceDelimiter}{i}{replaceDelimiter}", token);
                i++;
            }
            return text;
        }
    }
}
//#endif
