using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scombroid.LINQPadBlog.Utils;

namespace Scombroid.LINQPadBlog.Tests.Utils
{
    [TestClass]
    public class LinqPadScriptUtilsTests
    {
        [TestMethod]
        public void SuccessfullyLoadsCSharpExpressionScript()
        {
            var processedArgs = ProcessedArgs.ProcessScriptArgs(new string[] { TestData.CSharpExpression, "TestType1" });

            var scriptInfo = LinqPadScriptUtils.LoadLINQPadScriptInfo(processedArgs);

            Assert.IsNotNull(scriptInfo);
            Assert.AreEqual(Globals.LINQPad.QueryKind.CSharpExpression, scriptInfo.QueryKind);
            Assert.AreEqual(TestData.ExpectedTestScriptOuput, scriptInfo.ScriptOutput);
        }

        [TestMethod]
        public void SuccessfullyLoadsCSharpStatementsScript()
        {
            var processedArgs = ProcessedArgs.ProcessScriptArgs(new string[] { TestData.CSharpStatements, "TestType1" });

            var scriptInfo = LinqPadScriptUtils.LoadLINQPadScriptInfo(processedArgs);

            Assert.IsNotNull(scriptInfo);
            Assert.AreEqual(Globals.LINQPad.QueryKind.CSharpStatements, scriptInfo.QueryKind);
            Assert.AreEqual(TestData.ExpectedTestScriptOuput, scriptInfo.ScriptOutput);
        }

        [TestMethod]
        public void SuccessfullyLoadsCSharpProgramScript()
        {
            var processedArgs = ProcessedArgs.ProcessScriptArgs(new string[] { TestData.CSharpProgram, "TestType1" });

            var scriptInfo = LinqPadScriptUtils.LoadLINQPadScriptInfo(processedArgs);

            Assert.IsNotNull(scriptInfo);
            Assert.AreEqual(Globals.LINQPad.QueryKind.CSharpProgram, scriptInfo.QueryKind);
            Assert.AreEqual(TestData.ExpectedTestScriptOuput, scriptInfo.ScriptOutput);
        }
    }
}
