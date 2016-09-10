using System;
using HtmlAgilityPack;
using LINQPad;

namespace Scombroid.LINQPadBlog.Utils
{
    public class LinqPadWebResources
    {
        public string Styles { get; set; }
        public string Scripts { get; set; }
        public HtmlDocument LinqPadHtmlDoc { get; set; }

        public static LinqPadWebResources Generate()
        {
            var linqPadWebResources = new LinqPadWebResources();
            linqPadWebResources.LinqPadHtmlDoc = GetLINQPadOutputDOM();

            var styleNode = linqPadWebResources.LinqPadHtmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.StyleNodePath);
            linqPadWebResources.Styles = styleNode.InnerHtml.Trim();

            var scriptNode = linqPadWebResources.LinqPadHtmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.ScriptNodePath);
            linqPadWebResources.Scripts = scriptNode.InnerHtml.Trim();

            return linqPadWebResources;
        }

        private static HtmlDocument GetLINQPadOutputDOM()
        {
            string html = null;
            using (var tw = Util.CreateXhtmlWriter(true, false))
            {
                // LINQPad's CreateXhtmlWriter appears to have a "bug" in ToString(), in that 
                // it returns empty if nothing has been written yet.  To get around this, we write "true"
                // and then remove it after we've extracted the DOM 
                tw.Write(true);
                html = tw.ToString().Replace(Globals.LINQPad.TempDOMString, String.Empty);
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            return htmlDoc;
        }
    }
}
