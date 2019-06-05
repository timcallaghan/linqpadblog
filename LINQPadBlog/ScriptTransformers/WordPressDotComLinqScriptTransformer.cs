using HeyRed.MarkdownSharp;
using HtmlAgilityPack;
using Scombroid.LINQPadBlog.Utils;
using System;
using System.Net;
using System.Text;
using WordPressSharp;
using WordPressSharp.Models;

namespace Scombroid.LINQPadBlog.ScriptTransformers
{
    public class WordPressDotComLinqScriptTransformer : ILinqScriptTransformer
    {
        public WordPressDotComLinqScriptTransformer()
        {

        }

        public IScriptTransformResult Transform(LinqPadScriptInfo scriptInfo, IScriptTransformParams scriptParams)
        {
            var result = new ScriptTransformResult();

            var postParams = scriptParams as WordPressDotComParams;
            if (postParams == null)
                throw new ArgumentException($"{nameof(scriptParams)} must be an instance of {nameof(WordPressDotComParams)}");

            // Convert markdown to html
            var convertedScript = ConvertScriptContentsToHtml(scriptInfo);

            var output = BuildHtmlContents(scriptInfo, convertedScript);

            result.Location = UploadBlogPostToWordPress(output, postParams);

            return result;
        }

        private HtmlDocument BuildHtmlContents(LinqPadScriptInfo scriptInfo, string scriptHtml)
        {
            var htmlDoc = scriptInfo.LinqPadWebResources.LinqPadHtmlDoc;
            var body = htmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.BodyNodePath);

            var mainBody = htmlDoc.CreateTextNode(scriptHtml);
            body.AppendChild(mainBody);

            var dumpBody = htmlDoc.CreateTextNode(scriptInfo.ScriptOutput);
            body.AppendChild(dumpBody);

            return htmlDoc;
        }

        string UploadBlogPostToWordPress(HtmlDocument htmlDoc, WordPressDotComParams postParams)
        {
            // TODO: Remove dependency on WordPressClient and use Wordpress.com API instead
            var wpsc = new WordPressSiteConfig()
            {
               BaseUrl = postParams.BaseUrl,
               BlogId = postParams.BlogId,
               Username = postParams.Username,
               Password = postParams.Password
            };

            // TODO If postID is not null, ensure we "update" rather than create

            // Extract the <body> content
            var body = htmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.BodyNodePath);
            string content = body.InnerHtml;

            var post = new Post
            {
                PostType = postParams.PostType,
                Title = postParams.PostTitle,
                Content = content,
                PublishDateTime = DateTime.UtcNow,
                Status = postParams.PostStatus
            };

            using (var wpc = new WordPressClient(wpsc))
            {
                var id = Convert.ToInt32(wpc.NewPost(post));
                // TODO: Write the id to a postinfo.xml file so that
                // we can update the post by id in future

                // NOTE: WordPressClient.GetPost(id) fails due to outdated libs
                // Better to use Wordpress.com API (rather than XmlRPC) anyway
                // NOTE: Unfortunately, this all needs to be synchronous for now due to possible LINQPad limitations 
                // TODO: Try and make it async...

                using (var webClient = new WebClient())
                {
                    var blogSite = new Uri(postParams.BaseUrl);
                    var postJson = webClient.DownloadString($"{Globals.WordPressCom.BaseAPIUri}{blogSite.Host}/posts/{id}");
                    dynamic uploadedPost = Newtonsoft.Json.JsonConvert.DeserializeObject(postJson);
                    return uploadedPost.URL;
                }
            }
        }

        private string ConvertScriptContentsToHtml(LinqPadScriptInfo scriptInfo)
        {
            Markdown markdown = new Markdown();
            var result = new StringBuilder();
            var lang = Globals.WordPressCom.Syntax.csharp;
            switch (scriptInfo.QueryKind)
            {
                case Globals.LINQPad.QueryKind.VBExpression:
                case Globals.LINQPad.QueryKind.VBStatements:
                case Globals.LINQPad.QueryKind.VBProgram:
                    lang = Globals.WordPressCom.Syntax.vb;
                    break;
                case Globals.LINQPad.QueryKind.FSharpExpression:
                case Globals.LINQPad.QueryKind.FSharpProgram:
                    lang = Globals.WordPressCom.Syntax.fsharp;
                    break;
                case Globals.LINQPad.QueryKind.SQL:
                    lang = Globals.WordPressCom.Syntax.sql;
                    break;
            }

            var codeSectionStart = string.Format(Globals.WordPressCom.CodeSectionStart, lang);

            foreach (var section in scriptInfo.ScriptContents)
            {
                if (String.IsNullOrWhiteSpace(section.Contents))
                    continue;

                switch (section.ContentType)
                {
                    case ScriptContentSectionType.CompiledCode:
                    case ScriptContentSectionType.NonCompiledCode:
                        result.AppendLine(codeSectionStart);
                        result.AppendLine(WebUtility.HtmlEncode(section.Contents));
                        result.AppendLine(Globals.WordPressCom.CodeSectionEnd);
                        break;
                    case ScriptContentSectionType.DumpOutput:
                        // TODO: Implement dump lookup
                        break;
                    case ScriptContentSectionType.MarkdownComment:
                        // Replace inline math tags with latex tags used on wordpress.com
                        var multilineComment = section.Contents
                            .Replace(Globals.LATEX.InlineMathStart, $"{Globals.WordPressCom.LATEXMarker}{Globals.WordPressCom.LATEX} ")
                            .Replace(Globals.LATEX.InlineMathEnd, Globals.WordPressCom.LATEXMarker);

                        // Replace stand-alone math tags with latex tags used on wordpress.com
                        bool isStartOfTag = true;
                        var index = multilineComment.IndexOf(Globals.LATEX.StandAloneMathMarker);
                        while (index >= 0)
                        {
                            var before = multilineComment.Substring(0, index);
                            var middle = isStartOfTag ? string.Format("{0}{1} ", Globals.LATEX.IntermediateDollarSign, Globals.WordPressCom.LATEX) : Globals.LATEX.IntermediateDollarSign;
                            var after = index + 2 < multilineComment.Length ? multilineComment.Substring(index + 2) : string.Empty;

                            multilineComment = before + middle + after;
                            isStartOfTag = !isStartOfTag;
                            index = multilineComment.IndexOf(Globals.LATEX.StandAloneMathMarker);
                        }

                        multilineComment = multilineComment.Replace(Globals.LATEX.IntermediateDollarSign, Globals.WordPressCom.LATEXMarker);

                        result.AppendLine(markdown.Transform(multilineComment));
                        break;
                }
            }

            return result.ToString();
        }
    }
}
