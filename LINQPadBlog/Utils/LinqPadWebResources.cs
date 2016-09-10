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
                // LINQPad (v4) CreateXhtmlWriter appears to have a "bug" in ToString(), in that 
                // it returns empty if nothing has been written yet.  To get around this, we write "true" before calling ToString()
                tw.Write(true);

                // LINQPad (v5) appears to have a "bug" in ToString(), in that 
                // it duplicates the html document inside the body tag 
                // (perhaps Tostring() is not meant to be called on it?).
                // To get around this, we simply find the first occurence of <body>
                // and take all text up to that point (plus additional text to make the DOM well formed)
                html = tw.ToString();
                html = html.Substring(0, html.IndexOf(@"<body>")) + @"<body></body></html>";
            }

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            return htmlDoc;
        }
    }
}
