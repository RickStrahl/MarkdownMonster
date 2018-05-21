
using System;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnippetsAddin;

namespace SnippetsAddinTests
{
    public class Globals
    {
        public string Snippet;
        public string Name;
    }



[TestClass]
    public class ScriptParserTests
    {

        [TestMethod]
        public void EvalScript()
        {
            string script = @"Hi {{state.Name}},

Time is {{DateTime.Now.ToString(""MMM dd, yyyy HH:mm:ss"")}}
";

            var parser = new ScriptParser();
            string result =  parser.EvaluateScript(script, new Globals {Name = "Rick"});

            Assert.IsNotNull(result, "Parser failed to parse script: " + parser.ErrorMessage);
            Console.WriteLine(result);
        }

        [TestMethod]
        public void EvalScriptWithExpressionError()
        {
            string script = @"
Hi {{state.Name}},

Time is {{DateTime.sNow.ToString(""MMM dd, yyyy HH:mm:ss"")}}
";

            var parser = new ScriptParser();
            string result = parser.EvaluateScript(script, new Globals { Name = "Rick" });

            Assert.IsNull(result, "Parser should have returned null due to the error expression.");

        }


        [TestMethod]
        public void EvalScriptSync()
        {
            string script = @"
Hi {{Name}},

Time is {{DateTime.Now.ToString(""MMM dd, yyyy HH:mm:ss"")}}
";



            var parser = new ScriptParser();
            string result = parser.EvaluateScript(script, new Globals {Name = "Rick"});

            Console.WriteLine(result);

        }


        [TestMethod]
        public void EvalScriptRazor()
        {
            string script = @"Hi @Model.Name,

Time is @DateTime.Now.ToString(""MMM dd, yyyy HH:mm:ss"")
";

            var parser = new ScriptParser();
            string result = parser.EvaluateRazorScript(script, new Globals { Name = "Rick" });

            Assert.IsNotNull(result, "Parser failed to parse script: " + parser.ErrorMessage);
            Console.WriteLine(result);
        }

        [TestMethod]
        public void EvalScriptRazor2()
        {
            
            string script = @"Hi @Model.Name,

@for(int x=1;  x < 10; x++) {
     <text>Item @x</text>
}

Time is @DateTime.Now.ToString(""MMM dd, yyyy HH:mm:ss"")
";

            var parser = new ScriptParser();
            string result = parser.EvaluateRazorScript(script, new Globals { Name = "Rick" });

            Assert.IsNotNull(result, "Parser failed to parse script: " + parser.ErrorMessage);
            Console.WriteLine(result);
        }

#if false  // Roslyn scripting
        [TestMethod]
        public async Task ScriptTest()
        {

            string snippet = @"Time is: {DateTime.Now}. Os = {System.Environment.CommandLine}.";


            snippet = snippet.Replace("{", "\" + ").Replace("}", " + @\"");
            snippet = "@\"" + snippet + "\"";

            Console.WriteLine(snippet);

            var script =  snippet;

            Console.WriteLine("---");
            Console.WriteLine(script);

            string snip = @"Time is: " + DateTime.Now + @". Os = " + System.Environment.CommandLine + @".";
            Console.WriteLine("---");
            Console.WriteLine(snip);


            var options = ScriptOptions.Default;
            //Add reference to mscorlib
            var mscorlib = typeof(System.Object).Assembly;
            var systemCore = typeof(System.Linq.Enumerable).Assembly;
            options = options                
                .AddReferences(mscorlib, systemCore)
                .AddImports("System")
                .AddImports("System.IO")
                .AddImports("System.Text")
                .AddImports("System.Net")
                .AddImports("System.Linq")
                .AddImports("System.Collections.Generic");


            //note: we block here, because we are in Main method, normally we could await as scripting APIs are async
            var result = CSharpScript.EvaluateAsync<string>(script,options).Result;

            //result is now 5
            Console.WriteLine(result);
        }
#endif
    }
}
