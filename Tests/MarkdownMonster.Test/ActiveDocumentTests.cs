using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkdownMonster.Test
{
    [TestClass]
    public class ActiveDocumentTests
    {
        [TestMethod]
        public void ActiveDocumentTitleFromH1Tag()
        {

            var doc = new MarkdownDocument();

            doc.CurrentText = @"# Getting Started is hard to do

This is a test document
";

            Assert.IsTrue(doc.Title == "Getting Started is hard to do", "Title not found in # header");
            Console.WriteLine(doc.Title);
        }

        [TestMethod]
        public void ActiveDocumentTitleFromH1TagBuried()
        {

            var doc = new MarkdownDocument();

            doc.CurrentText = @"

# Getting Started is hard to do

This is a test document
";


            Console.WriteLine(doc.Title);
            Assert.IsTrue(doc.Title == "Getting Started is hard to do", "Title not found in # header");            
        }

        [TestMethod]
        public void ActiveDocumentTitleFromFrontMatter()
        {

            var doc = new MarkdownDocument();

            doc.CurrentText = @"---
title: Markdown Monster ToDo List
date: 2017-01-27
tags: 
- test
- test2
---
This is a test document
";
            Console.WriteLine(doc.Title);
            Assert.IsTrue(doc.Title == "Markdown Monster ToDo List", "Title not found in # header");
            
        }

        [TestMethod]
        public void ActiveDocumentFromFilename()
        {

            var doc = new MarkdownDocument();
            doc.Filename = "c:\\temp\\GettingStartedIsHardToDo.md";
            
            Assert.IsTrue(doc.Title == "Getting Started Is Hard To Do", "Title not converted from Camel Case");
            Console.WriteLine(doc.Title);
        }
    }
}
