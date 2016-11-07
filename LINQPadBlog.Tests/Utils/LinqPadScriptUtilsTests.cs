using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scombroid.LINQPadBlog.Utils;

namespace Scombroid.LINQPadBlog.Tests.Utils
{
    [TestClass]
    public class LinqPadScriptUtilsTests
    {
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void FailsToLoadCSharpExpressionScript()
        {
            var processedArgs = ProcessedArgs.ProcessScriptArgs(new string[] { TestData.CSharpExpression });
            using (var tempFile = new TempFileManager(processedArgs.FilePath))
            {
                LinqPadScriptUtils.LoadLINQPadScriptInfo(tempFile, processedArgs, null);
            }
        }

        [TestMethod]
        public void SuccessfullyLoadsCSharpStatementsScript()
        {
            var processedArgs = ProcessedArgs.ProcessScriptArgs(new string[] { TestData.CSharpStatements });
            using (var tempFile = new TempFileManager(processedArgs.FilePath))
            {
                var scriptInfo = LinqPadScriptUtils.LoadLINQPadScriptInfo(tempFile, processedArgs, null);

                Assert.IsNotNull(scriptInfo);
                Assert.AreEqual(Globals.LINQPad.QueryKind.CSharpStatements, scriptInfo.QueryKind);
            }
        }

        [TestMethod]
        public void SuccessfullyLoadsCSharpProgramScript()
        {
            var processedArgs = ProcessedArgs.ProcessScriptArgs(new string[] { TestData.CSharpProgram });
            using (var tempFile = new TempFileManager(processedArgs.FilePath))
            {
                var scriptInfo = LinqPadScriptUtils.LoadLINQPadScriptInfo(tempFile, processedArgs, null);

                Assert.IsNotNull(scriptInfo);
                Assert.AreEqual(Globals.LINQPad.QueryKind.CSharpProgram, scriptInfo.QueryKind);
            }
        }

        [TestMethod]
        public void SuccessfullyLoadsFSharpProgramScript()
        {
            var processedArgs = ProcessedArgs.ProcessScriptArgs(new string[] { TestData.FSharpProgram });
            using (var tempFile = new TempFileManager(processedArgs.FilePath))
            {
                var scriptInfo = LinqPadScriptUtils.LoadLINQPadScriptInfo(tempFile, processedArgs, null);

                Assert.IsNotNull(scriptInfo);
                Assert.AreEqual(Globals.LINQPad.QueryKind.FSharpProgram, scriptInfo.QueryKind);
            }
        }
    }
}
