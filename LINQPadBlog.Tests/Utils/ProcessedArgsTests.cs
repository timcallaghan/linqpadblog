using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scombroid.LINQPadBlog.Utils;

namespace Scombroid.LINQPadBlog.Tests.Utils
{
    [TestClass]
    public class ProcessedArgsTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentExceptionWhenInputIsNull()
        {
            ProcessedArgs.ProcessScriptArgs(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentExceptionWhenInputOnlyHasOneElement()
        {
            ProcessedArgs.ProcessScriptArgs(new string[] { "arg1" });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentExceptionWhenFirstInputIsNotAFilePath()
        {
            ProcessedArgs.ProcessScriptArgs(new string[] { "arg1", "arg2" });
        }

        [TestMethod]
        public void SuccessfullyProcessesTwoArgs()
        {
            var generatorType = "Type1";

            var pa = ProcessedArgs.ProcessScriptArgs(new string[] { TestData.CSharpExpression, generatorType });

            Assert.IsNotNull(pa.FilePath);
            Assert.IsTrue(pa.FilePath.FullName.EndsWith(TestData.CSharpExpression));
            Assert.AreEqual(generatorType, pa.GeneratorType);
            Assert.IsNull(pa.AdditionalArgs);
        }

        [TestMethod]
        public void SuccessfullyProcessesMoreThanTwoArgs()
        {
            var generatorType = "Type1";
            var arg3 = "arg3";
            var arg4 = "arg4";

            var pa = ProcessedArgs.ProcessScriptArgs(new string[] { TestData.CSharpExpression, generatorType, arg3, arg4 });

            Assert.IsNotNull(pa.FilePath);
            Assert.IsTrue(pa.FilePath.FullName.EndsWith(TestData.CSharpExpression));
            Assert.AreEqual(generatorType, pa.GeneratorType);
            Assert.IsNotNull(pa.AdditionalArgs);
            Assert.AreEqual(2, pa.AdditionalArgs.Length);
            Assert.AreEqual(arg3, pa.AdditionalArgs[0]);
            Assert.AreEqual(arg4, pa.AdditionalArgs[1]);
        }
    }
}
