using HeyRed.MarkdownSharp;
using HtmlAgilityPack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Scombroid.LINQPadBlog.Utils;
using System;
using System.Net;
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
            var tree = CSharpSyntaxTree.ParseText(scriptInfo.ScriptContents);
            SyntaxNode root = tree.GetRoot();
            var rewriter = new CommentRewriter(scriptInfo.QueryKind);
            var newRoot = rewriter.Visit(root);

            var output = BuildHtmlContents(scriptInfo, newRoot.ToFullString(), rewriter.CodeSectionStart);

            result.Location = UploadBlogPostToWordPress(output, postParams);

            return result;
        }

        private HtmlDocument BuildHtmlContents(LinqPadScriptInfo scriptInfo, string scriptHtml, string codeSectionStart)
        {
            var htmlDoc = scriptInfo.LinqPadWebResources.LinqPadHtmlDoc;

            HtmlNode head = htmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.HeadNodePath);
            var body = htmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.BodyNodePath);

            if (scriptHtml.StartsWith(Globals.WordPressCom.CodeSectionEnd))
            {
                scriptHtml = scriptHtml.Substring(Globals.WordPressCom.CodeSectionEnd.Length);
            }

            if (scriptHtml.EndsWith(codeSectionStart))
            {
                scriptHtml = scriptHtml.Substring(0, scriptHtml.Length - codeSectionStart.Length);
            }
            else
            {
                scriptHtml += Globals.WordPressCom.CodeSectionEnd;
            }

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

        private class CommentRewriter : CSharpSyntaxRewriter
        {
            private Markdown mark = new Markdown();
            public string CodeSectionStart { get; private set; }

            public CommentRewriter(string queryKind) : base()
            {
                var lang = Globals.WordPressCom.Syntax.csharp;
                switch (queryKind)
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

                CodeSectionStart = string.Format(Globals.WordPressCom.CodeSectionStart, lang);
            }

            public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
            {
                // By convention, LINQPadBlog only looks for markdown in multi-line trivia
                if (trivia.Kind() == SyntaxKind.MultiLineCommentTrivia)
                {
                    // Remove the leading "/*" from the comment
                    var multilineComment = trivia.ToString().Substring(2);
                    // Remove the trailing "*//r/n" from the comment
                    // TODO: Ensure comment finish at EOF doesn't screw things up...
                    multilineComment = multilineComment.Substring(0, multilineComment.Length - 3);

                    // Replace inline math tags with latex tags used on wordpress.com
                    multilineComment = multilineComment.Replace(Globals.LATEX.InlineMathStart, string.Format("{0}{1} ", Globals.WordPressCom.LATEXMarker, Globals.WordPressCom.LATEX))
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

                    // Assumes that all multi-line comments are embedded between a block of code 
                    // (should be true in general except at the start or end of the file, which is handled elsewhere)
                    // Use Markdown to transform any embedded markdown in the comments to html
                    return SyntaxFactory.Comment(Globals.WordPressCom.CodeSectionEnd + mark.Transform(multilineComment) + CodeSectionStart);
                }
                else
                {
                    return base.VisitTrivia(trivia);
                }
            }
        }
    }
}
