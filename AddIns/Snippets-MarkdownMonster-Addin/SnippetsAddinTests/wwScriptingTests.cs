using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnippetsAddin;
using Westwind.wwScripting;

namespace SnippetsAddinTests
{




[TestClass]
    public class wwScriptingTests
    {

        [TestMethod]
        public void EvalScript()
        {
  

            var scripting = new wwScripting();
            string result = scripting.ExecuteCode("return \"Hi Rick\";") as string;

            Console.WriteLine(scripting.ErrorMessage);
            Console.WriteLine(result);

        }

        [TestMethod]
        public void EvalScrip2()
        {

            var globals = new Globals();

            string code = @"Console.WriteLine(Parameters.Length);
            dynamic state = Parameters[0];
            return @""<small>created with <a href=""""http://markdownmonster.west-wind.com"""">Markdown Monster "" + DateTime.Now + @""</small>"";";
            
            var scripting = new wwScripting();
            scripting.SaveSourceCode = true;
            string result = scripting.ExecuteCode(code,globals) as string;


            Console.WriteLine(scripting.ErrorMessage);
            Console.WriteLine(result);
            Console.WriteLine(scripting.SourceCode);

        }

    }
}
