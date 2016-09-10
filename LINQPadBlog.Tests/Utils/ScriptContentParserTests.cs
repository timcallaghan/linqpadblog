using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scombroid.LINQPadBlog.Utils;

namespace Scombroid.LINQPadBlog.Tests.Utils
{
    [TestClass]
    public class ScriptContentParserTests
    {
        [TestMethod]
        public void CorrectlyParsesCSharpScript()
        {
            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, GenerateCSharpTestScript(), null);
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

        private string GenerateCSharpTestScript()
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
    }
}