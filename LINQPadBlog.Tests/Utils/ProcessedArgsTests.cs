﻿using System;
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
        public void ThrowsArgumentExceptionWhenInputIsEmpty()
        {
            ProcessedArgs.ProcessScriptArgs(new string[] { });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void ThrowsArgumentExceptionWhenFirstInputIsNotAFilePath()
        {
            ProcessedArgs.ProcessScriptArgs(new string[] { "arg1" });
        }

        [TestMethod]
        public void SuccessfullyProcessesOneValidArg()
        {
            var pa = ProcessedArgs.ProcessScriptArgs(new string[] { TestData.CSharpExpression });

            Assert.IsNotNull(pa.FilePath);
            Assert.IsTrue(pa.FilePath.FullName.EndsWith(TestData.CSharpExpression));
            Assert.IsNull(pa.AdditionalArgs);
        }

        [TestMethod]
        public void SuccessfullyProcessesMoreThanOneValidArg()
        {
            var arg2 = "arg2";
            var arg3 = "arg3";

            var pa = ProcessedArgs.ProcessScriptArgs(new string[] { TestData.CSharpExpression, arg2, arg3 });

            Assert.IsNotNull(pa.FilePath);
            Assert.IsTrue(pa.FilePath.FullName.EndsWith(TestData.CSharpExpression));
            Assert.IsNotNull(pa.AdditionalArgs);
            Assert.AreEqual(2, pa.AdditionalArgs.Length);
            Assert.AreEqual(arg2, pa.AdditionalArgs[0]);
            Assert.AreEqual(arg3, pa.AdditionalArgs[1]);
        }
    }
}
