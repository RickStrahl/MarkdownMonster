using System;
using System.Collections.Generic;
using Markdig;
using Markdig.Helpers;
using Markdig.Syntax;
using MarkdownMonster.Windows.ConfigurationEditor;
using MarkdownMonster.Windows.DocumentOutlineSidebar;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Westwind.TypeImporter;
using Westwind.Utilities;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class ConfigurationParserTests
    {
        [TestMethod]
        public void ParseAppConfiguration()
        {
            var parser = new ConfigurationParser();
            var dotnetObject = parser.ParseConfigurationObject(typeof(ApplicationConfiguration));

            RenderType(dotnetObject);
        }

        [TestMethod]
        public void ParseAllConfigurationObjectsTest()
        {
            var parser = new ConfigurationParser();

            parser.ParseAllConfigurationObjects();

            var list = parser.FindProperty("Theme");

            RenderPropertyItems(list);
        }


        void RenderType(DotnetObject type)
        {
            Console.WriteLine($"{type} -  - {type.Signature}");


            if (type.Properties.Count > 0)
            {
                Console.WriteLine("  *** Properties:");
                foreach (var prop in type.Properties)
                {
                    Console.WriteLine($"* {prop}  -  {prop.Signature}");
                    Console.WriteLine($"{prop.HelpText}");
                    Console.WriteLine("---");
                }
            }


        }

        void RenderPropertyItems(List<ConfigurationPropertyItem> items)
        {
            foreach (var item in items)
            {
                Console.WriteLine(item.SectionDisplayName + " - " + item.Property.Name);
                Console.WriteLine(item.Property.HelpText);
                Console.WriteLine("---");
            }
        }
    }
}
