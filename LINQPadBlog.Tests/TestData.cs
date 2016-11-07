using System;
using System.IO;
using System.Net.NetworkInformation;

namespace Scombroid.LINQPadBlog.Tests
{
    static class TestData
    {
        public const string TestDataFolder = "TestData";
        public static string CSharpExpression = Path.Combine(TestDataFolder, "CSharpExpression.linq");
        public static string CSharpStatements = Path.Combine(TestDataFolder, "CSharpStatements.linq");
        public static string CSharpProgram = Path.Combine(TestDataFolder, "CSharpProgram.linq");
        public static string FSharpProgram = Path.Combine(TestDataFolder, "FSharpProgram.linq");
        // ReSharper disable once InconsistentNaming
        public static string LINQPadDOM = Path.Combine(TestDataFolder, "LINQPadDOM.txt");
        public static string ExpectedTestScriptOuput => $"<div><div>42</div>{Environment.NewLine}</div>";
    }
}
