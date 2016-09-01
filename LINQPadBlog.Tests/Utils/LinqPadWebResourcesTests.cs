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
            var webResources = LinqPadWebResources.Generate();

            Assert.IsNotNull(webResources);
            Assert.IsNotNull(webResources.LinqPadHtmlDoc);
            Assert.IsFalse(string.IsNullOrWhiteSpace(webResources.Scripts));
            Assert.IsFalse(string.IsNullOrWhiteSpace(webResources.Styles));
        }
    }
}
