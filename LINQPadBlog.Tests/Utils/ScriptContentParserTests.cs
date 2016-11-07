using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scombroid.LINQPadBlog.Utils;

namespace Scombroid.LINQPadBlog.Tests.Utils
{
    [TestClass]
    public class ScriptContentParserTests
    {
        [TestMethod]
        public void CorrectlyParsesCSharpTestScript1()
        {
            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, GenerateCSharpTestScript1(), null);
            Assert.AreEqual(10, scriptContentParser.ScriptContentSections.Count);
            Assert.AreEqual(ScriptContentSectionType.MarkdownComment, scriptContentParser.ScriptContentSections[0].ContentType);
            Assert.AreEqual(ScriptContentSectionType.NonCompiledCode, scriptContentParser.ScriptContentSections[1].ContentType);
            Assert.AreEqual(ScriptContentSectionType.MarkdownComment, scriptContentParser.ScriptContentSections[2].ContentType);
            Assert.AreEqual(ScriptContentSectionType.NonCompiledCode, scriptContentParser.ScriptContentSections[3].ContentType);
            Assert.AreEqual(ScriptContentSectionType.NonCompiledCode, scriptContentParser.ScriptContentSections[4].ContentType);
            Assert.AreEqual(ScriptContentSectionType.MarkdownComment, scriptContentParser.ScriptContentSections[5].ContentType);
            Assert.AreEqual(ScriptContentSectionType.CompiledCode, scriptContentParser.ScriptContentSections[6].ContentType);
            Assert.AreEqual(ScriptContentSectionType.MarkdownComment, scriptContentParser.ScriptContentSections[7].ContentType);
            Assert.AreEqual(ScriptContentSectionType.DumpOutput, scriptContentParser.ScriptContentSections[8].ContentType);
            Assert.AreEqual(ScriptContentSectionType.MarkdownComment, scriptContentParser.ScriptContentSections[9].ContentType);
        }

        [TestMethod]
        public void CorrectlyParsesCSharpTestScript2()
        {
            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, GenerateCSharpTestScript2(), null);
            Assert.AreEqual(2, scriptContentParser.ScriptContentSections.Count);
            Assert.AreEqual(ScriptContentSectionType.MarkdownComment, scriptContentParser.ScriptContentSections[0].ContentType);
            Assert.AreEqual(ScriptContentSectionType.NonCompiledCode, scriptContentParser.ScriptContentSections[1].ContentType);
        }

        [TestMethod]
        public void CorrectlyParsesCSharpTestScript3()
        {
            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, GenerateCSharpTestScript3(), null);
            Assert.AreEqual(2, scriptContentParser.ScriptContentSections.Count);
            Assert.AreEqual(ScriptContentSectionType.MarkdownComment, scriptContentParser.ScriptContentSections[0].ContentType);
            Assert.AreEqual(ScriptContentSectionType.NonCompiledCode, scriptContentParser.ScriptContentSections[1].ContentType);
        }

        [TestMethod]
        public void CorrectlyParsesCSharpTestScript4()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine(Globals.Comments.DumpStart);
            input.AppendLine(Globals.Comments.DumpEnd);
            input.AppendLine(Globals.Comments.CSharpEnd);

            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, input.ToString(), null);
            Assert.AreEqual(3, scriptContentParser.ScriptContentSections.Count);
            Assert.AreEqual(ScriptContentSectionType.MarkdownComment, scriptContentParser.ScriptContentSections[0].ContentType);
            Assert.AreEqual(ScriptContentSectionType.DumpOutput, scriptContentParser.ScriptContentSections[1].ContentType);
            Assert.AreEqual(ScriptContentSectionType.CompiledCode, scriptContentParser.ScriptContentSections[2].ContentType);
        }

        [TestMethod]
        public void CorrectlyParsesCSharpTestScript5()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine(Globals.Comments.DumpStart);
            input.AppendLine(Globals.Comments.DumpEnd);
            // Note: This is using Append (not AppendLine) so we expect one less section
            input.Append(Globals.Comments.CSharpEnd);

            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, input.ToString(), null);
            Assert.AreEqual(2, scriptContentParser.ScriptContentSections.Count);
            Assert.AreEqual(ScriptContentSectionType.MarkdownComment, scriptContentParser.ScriptContentSections[0].ContentType);
            Assert.AreEqual(ScriptContentSectionType.DumpOutput, scriptContentParser.ScriptContentSections[1].ContentType);
        }

        [TestMethod]
        public void CorrectlyParsesCSharpTestScript6()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine(Globals.Comments.DumpStart);
            input.AppendLine(Globals.Comments.DumpEnd);
            // Note: This is using Append (not AppendLine) so we expect one less section
            input.Append(Globals.Comments.CSharpEnd);

            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, GenerateCSharpTestScript4(), null);

            Assert.AreEqual(2, scriptContentParser.ScriptContentSections.Count);
            Assert.AreEqual(ScriptContentSectionType.MarkdownComment, scriptContentParser.ScriptContentSections[0].ContentType);
            Assert.AreEqual(ScriptContentSectionType.CompiledCode, scriptContentParser.ScriptContentSections[1].ContentType);
        }

        private string GenerateCSharpTestScript1()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine("Comment line 1");
            input.AppendLine("Comment line 2");
            input.AppendLine("Comment line 3");
            input.AppendLine(Globals.Comments.NonCompiledCodeStart);
            input.AppendLine("var date1 = new DateTime();");
            input.AppendLine(Globals.Comments.NonCompiledCodeEnd);
            input.AppendLine("Comment line 4");
            input.AppendLine("Comment line 5");
            input.AppendLine("Comment line 6");
            input.AppendLine(Globals.Comments.NonCompiledCodeStart);
            input.AppendLine("var date2 = new DateTime();");
            input.AppendLine(Globals.Comments.NonCompiledCodeEnd);
            input.AppendLine(Globals.Comments.NonCompiledCodeStart);
            input.AppendLine("var date3 = new DateTime();");
            input.AppendLine(Globals.Comments.NonCompiledCodeEnd);
            input.AppendLine("Comment line 7");
            input.AppendLine("Comment line 8");
            input.AppendLine("Comment line 9");
            input.AppendLine(Globals.Comments.CSharpEnd);
            input.AppendLine("var date3 = new DateTime();");
            input.AppendLine("date3.Dump(\"Ouput1\");");
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine("Comment line 10");
            input.AppendLine(Globals.Comments.DumpStart);
            input.AppendLine("Ouput1");
            input.AppendLine(Globals.Comments.DumpEnd);
            input.AppendLine("Comment line 11");
            input.AppendLine("Comment line 12");
            input.Append(Globals.Comments.CSharpEnd);

            return input.ToString();
        }

        private string GenerateCSharpTestScript2()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine("Comment line 1");
            input.AppendLine("Comment line 2");
            input.AppendLine("Comment line 3");
            input.AppendLine(Globals.Comments.NonCompiledCodeStart);
            input.AppendLine("var date1 = new DateTime();");
            input.AppendLine(Globals.Comments.NonCompiledCodeEnd);
            input.Append(Globals.Comments.CSharpEnd);

            return input.ToString();
        }

        private string GenerateCSharpTestScript3()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine(Globals.Comments.NonCompiledCodeStart);
            input.AppendLine("var date1 = new DateTime();");
            input.AppendLine(Globals.Comments.NonCompiledCodeEnd);
            input.Append(Globals.Comments.CSharpEnd);

            return input.ToString();
        }

        private string GenerateCSharpTestScript4()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine("# LINQPadBlog - Blogging from LINQPad #");
            input.AppendLine("Here is some opening text, in which I explain how this cool code commenting technique works and ");
            input.AppendLine("how awesome LINQPad is for doing this kind of stuff.  ");
            input.AppendLine("*This is italicized*, and so is _this_.");
            input.AppendLine("**This is bold**, and so is __this__.");
            input.AppendLine("Use ***italics and bold together*** if you ___have to___.");
            input.AppendLine("");
            input.AppendLine("The `$` character is just a shortcut for `window.jQuery`.");
            input.AppendLine("");
            input.AppendLine("Here is some math via Mathjax/LaTeX...  ");
            input.AppendLine(@"When \\(a \ne 0\\), there are two solutions to \\(ax^2 + bx + c = 0\\) and they are");
            input.AppendLine(@"$$x = {-b \pm \sqrt{b^2-4ac} \over 2a}.$$");
            input.AppendLine(Globals.Comments.CSharpEnd);
            input.Append("void Main()");

            return input.ToString();
        }
    }
}