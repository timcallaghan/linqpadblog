using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scombroid.LINQPadBlog.Utils;

namespace Scombroid.LINQPadBlog.Tests.Utils
{
    [TestClass]
    public class ScriptContentParserExceptionTests
    {
        [TestMethod]
        [ExpectedException(typeof(ScriptContentParseException))]
        public void ThrowsExceptionWhenMissingNonCompiledCodeStart()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine("Comment line 1");
            input.AppendLine(Globals.Comments.NonCompiledCodeEnd);
            input.Append(Globals.Comments.CSharpEnd);

            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, input.ToString(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ScriptContentParseException))]
        public void ThrowsExceptionWhenMissingDumpStart()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine("Comment line 1");
            input.AppendLine(Globals.Comments.DumpEnd);
            input.Append(Globals.Comments.CSharpEnd);

            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, input.ToString(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ScriptContentParseException))]
        public void ThrowsExceptionWhenTwoNonCompiledCodeStarts()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine("Comment line 1");
            input.AppendLine(Globals.Comments.NonCompiledCodeStart);
            input.AppendLine(Globals.Comments.NonCompiledCodeStart);
            input.Append(Globals.Comments.CSharpEnd);

            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, input.ToString(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ScriptContentParseException))]
        public void ThrowsExceptionWhenTwoDumpStarts()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine("Comment line 1");
            input.AppendLine(Globals.Comments.DumpStart);
            input.AppendLine(Globals.Comments.DumpStart);
            input.Append(Globals.Comments.CSharpEnd);

            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, input.ToString(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ScriptContentParseException))]
        public void ThrowsExceptionWhenDumpStartInsideNonCompiledCodeStart()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine("Comment line 1");
            input.AppendLine(Globals.Comments.NonCompiledCodeStart);
            input.AppendLine(Globals.Comments.DumpStart);
            input.Append(Globals.Comments.CSharpEnd);

            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, input.ToString(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ScriptContentParseException))]
        public void ThrowsExceptionWhenNonCompiledCodeStartInsideDumpStart()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine("Comment line 1");
            input.AppendLine(Globals.Comments.DumpStart);
            input.AppendLine(Globals.Comments.NonCompiledCodeStart);
            input.Append(Globals.Comments.CSharpEnd);

            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, input.ToString(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ScriptContentParseException))]
        public void ThrowsExceptionWhenNonCompiledCodeStartButDumpEnd()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine("Comment line 1");
            input.AppendLine(Globals.Comments.NonCompiledCodeStart);
            input.AppendLine(Globals.Comments.DumpEnd);
            input.Append(Globals.Comments.CSharpEnd);

            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, input.ToString(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ScriptContentParseException))]
        public void ThrowsExceptionWhenDumpStartButNonCompiledCodeEnd()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine("Comment line 1");
            input.AppendLine(Globals.Comments.DumpStart);
            input.AppendLine(Globals.Comments.NonCompiledCodeEnd);
            input.Append(Globals.Comments.CSharpEnd);

            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, input.ToString(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ScriptContentParseException))]
        public void ThrowsExceptionWhenMissingCommentEnd()
        {
            var input = new StringBuilder();
            input.AppendLine(Globals.Comments.CSharpStart);
            input.AppendLine("Comment line 1");

            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, input.ToString(), null);
        }

        [TestMethod]
        [ExpectedException(typeof(ScriptContentParseException))]
        public void ThrowsExceptionWhenMissingCommentStart()
        {
            var input = new StringBuilder();
            input.AppendLine("Comment line 1");
            input.Append(Globals.Comments.CSharpEnd);

            var scriptContentParser = new ScriptContentParser(Globals.Comments.CSharpStart, Globals.Comments.CSharpEnd, input.ToString(), null);
        }
    }
}