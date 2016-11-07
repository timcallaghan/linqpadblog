using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scombroid.LINQPadBlog.Utils;

namespace Scombroid.LINQPadBlog.Tests.Utils
{
    [TestClass]
    public class LinqPadWebResourcesTests
    {
        [TestMethod]
        public void SuccessfullyGeneratesWebResources()
        {
            var linqPadDOMFile = new FileInfo(TestData.LINQPadDOM);

            var webResources = LinqPadWebResources.Generate(File.ReadAllText(linqPadDOMFile.FullName));

            Assert.IsNotNull(webResources);
            Assert.IsNotNull(webResources.LinqPadHtmlDoc);
            Assert.IsFalse(string.IsNullOrWhiteSpace(webResources.Scripts));
            Assert.IsFalse(string.IsNullOrWhiteSpace(webResources.Styles));
        }
    }
}
