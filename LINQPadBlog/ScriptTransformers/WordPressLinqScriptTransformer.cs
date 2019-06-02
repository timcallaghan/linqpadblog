using HeyRed.MarkdownSharp;
using HtmlAgilityPack;
using Scombroid.LINQPadBlog.Utils;
using System;
using System.Net;
using System.Text;
using WordPressPCL;
using WordPressPCL.Models;

namespace Scombroid.LINQPadBlog.ScriptTransformers
{
    public class WordPressLinqScriptTransformer : ILinqScriptTransformer
    {
        public WordPressLinqScriptTransformer()
        {

        }

        public IScriptTransformResult Transform(LinqPadScriptInfo scriptInfo, IScriptTransformParams scriptParams)
        {
            var result = new ScriptTransformResult();

            var postParams = scriptParams as WordPressParams;
            if (postParams == null)
                throw new ArgumentException($"{nameof(scriptParams)} must be an instance of {nameof(WordPressParams)}");

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

        string UploadBlogPostToWordPress(HtmlDocument htmlDoc, WordPressParams postParams)
        {
            // TODO If postID is not null, ensure we "update" rather than create

            // Extract the <body> content
            var body = htmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.BodyNodePath);
            string content = body.InnerHtml;

            var post = new Post
            {
                Title = new Title(postParams.PostTitle),
                Content = new Content(content),
                DateGmt = DateTime.UtcNow,
                Status = Status.Publish, // TODO:Fix postParams.PostStatus
                Format = postParams.Format
            };

            // TODO: Find a way to make this async...
            var client = new WordPressClient(postParams.BaseApiUrl);
            client.AuthMethod = AuthMethod.JWT;
            client.RequestJWToken(postParams.Username, postParams.Password).Wait();
            // check if authentication has been successful TODO handle this...
            client.IsValidJWToken().Wait();

            var result = client.Posts.Create(post);
            result.Wait();
            var newPost = result.Result;

            return newPost.Link;
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

            var codeSectionStart = string.Format(Globals.WordPress.CodeSectionStart, lang);

            foreach (var section in scriptInfo.ScriptContents)
            {
                if (String.IsNullOrWhiteSpace(section.Contents))
                    continue;

                switch (section.ContentType)
                {
                    case ScriptContentSectionType.CompiledCode:
                    case ScriptContentSectionType.NonCompiledCode:
                        result.AppendLine(codeSectionStart);
                        result.AppendLine(section.Contents);
                        result.AppendLine(Globals.WordPress.CodeSectionEnd);
                        break;
                    case ScriptContentSectionType.DumpOutput:
                        // TODO: Implement dump lookup
                        break;
                    case ScriptContentSectionType.MarkdownComment:
                        result.AppendLine(markdown.Transform(section.Contents));
                        break;
                }
            }

            return result.ToString();
        }
    }
}
