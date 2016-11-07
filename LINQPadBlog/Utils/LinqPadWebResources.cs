using HtmlAgilityPack;

namespace Scombroid.LINQPadBlog.Utils
{
    public class LinqPadWebResources
    {
        public string Styles { get; set; }
        public string Scripts { get; set; }
        public HtmlDocument LinqPadHtmlDoc { get; set; }

        public static LinqPadWebResources Generate(string linqPadOutputDOM)
        {
            var linqPadWebResources = new LinqPadWebResources();

            linqPadWebResources.LinqPadHtmlDoc = new HtmlDocument();
            linqPadWebResources.LinqPadHtmlDoc.LoadHtml(linqPadOutputDOM);

            var styleNode = linqPadWebResources.LinqPadHtmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.StyleNodePath);
            linqPadWebResources.Styles = styleNode.InnerHtml.Trim();

            var scriptNode = linqPadWebResources.LinqPadHtmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.ScriptNodePath);
            linqPadWebResources.Scripts = scriptNode.InnerHtml.Trim();

            return linqPadWebResources;
        }
    }
}
