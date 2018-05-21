using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SnippetsAddin;

namespace SnippetsAddinTests
{
    /// <summary>
    /// Summary description for StringTokenization
    /// </summary>
    [TestClass]
    public class StringTokenization
    {
        public StringTokenization()
        {
            //
            // TODO: Add constructor logic here
            //
        }

  

        [TestMethod]
        public void TestMethod1()
        {
            string code = "This is a test {{DateTime.Now}}  and another {{System.Environment.CurrentDirectory}}";

            var parser = new ScriptParser();
            var tokens = parser.TokenizeString(ref code,"{{","}}");

            Console.WriteLine(code);

            foreach (var  token in tokens)
            {
                Console.WriteLine(token);

            }

            code = parser.DetokenizeString(code, tokens);

            Console.WriteLine("returned");
            Console.WriteLine(code);

        }
    }
}
