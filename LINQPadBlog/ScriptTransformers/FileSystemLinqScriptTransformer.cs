﻿using HeyRed.MarkdownSharp;
using HtmlAgilityPack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Scombroid.LINQPadBlog.Utils;
using System;
using System.IO;

namespace Scombroid.LINQPadBlog.ScriptTransformers
{
    public class FileSystemLinqScriptTransformer : ILinqScriptTransformer
    {
        private DirectoryInfo _resourcesDir { get; set; }
        private string LINQPadCssRelativePath { get { return $"{_resourcesDir.Name}/{Globals.FileSystem.LINQPadCssFileName}"; } }
        private string LINQPadJsRelativePath { get { return $"{_resourcesDir.Name}/{Globals.FileSystem.LINQPadJsFileName}"; } }
        private DirectoryInfo _blogDir;

        public FileSystemLinqScriptTransformer(DirectoryInfo blogDir)
        {
            if (blogDir == null)
                throw new ArgumentException($"{nameof(blogDir)} can't be null");

            _blogDir = blogDir;
        }

        public IScriptTransformResult Transform(LinqPadScriptInfo scriptInfo, IScriptTransformParams scriptParams)
        {
            var result = new ScriptTransformResult();

            // Create the Posts directory if it doesn't exist
            if (!_blogDir.Exists)
            {
                _blogDir.Create();
            }

            CreateWebPageResourceFiles(scriptInfo, _blogDir);

            // TODO: We need to generate different markup depending on the QueryKind
            // i.e. C#, VB, F# (each will need a different prettyprint lang- setting)

            // Convert markdown to html
            var tree = CSharpSyntaxTree.ParseText(scriptInfo.ScriptContents);
            SyntaxNode root = tree.GetRoot();
            var rewriter = new CommentRewriter();
            var newRoot = rewriter.Visit(root);

            // Get the output file path
            var outfile = new FileInfo(Path.Combine(_blogDir.FullName, Path.ChangeExtension(scriptInfo.ProcessedArgs.FilePath.Name, "html")));

            var output = BuildHtmlContents(scriptInfo, newRoot.ToFullString(), true);

            File.WriteAllText(outfile.FullName, output.DocumentNode.OuterHtml);

            result.Location = outfile.FullName;

            return result;
        }

        private void CreateWebPageResourceFiles(LinqPadScriptInfo scriptInfo, DirectoryInfo blogDir)
        {
            // Create the resources folder if it doesn't exist
            _resourcesDir = new DirectoryInfo(Path.Combine(blogDir.FullName, Globals.FileSystem.ResourcesFolderName));
            if (!_resourcesDir.Exists)
            {
                _resourcesDir.Create();
            }

            File.WriteAllText(Path.Combine(_resourcesDir.FullName, Globals.FileSystem.LINQPadCssFileName), scriptInfo.LinqPadWebResources.Styles);
            File.WriteAllText(Path.Combine(_resourcesDir.FullName, Globals.FileSystem.LINQPadJsFileName), scriptInfo.LinqPadWebResources.Scripts);
        }

        private HtmlDocument BuildHtmlContents(LinqPadScriptInfo scriptInfo, string scriptHtml, bool replaceResources)
        {
            var htmlDoc = scriptInfo.LinqPadWebResources.LinqPadHtmlDoc;
            htmlDoc.OptionWriteEmptyNodes = true;

            HtmlNode head = htmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.HeadNodePath);

            var titleNode = htmlDoc.CreateElement("title");
            titleNode.InnerHtml = Path.GetFileNameWithoutExtension(scriptInfo.ProcessedArgs.FilePath.Name);
            head.ChildNodes.Add(titleNode);

            if (replaceResources)
            {
                // Remove the style section and script section generated by LINQPad,
                // and replace with links to resource files
                var styleNode = htmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.StyleNodePath);
                var replacementStyleNode = htmlDoc.CreateElement("link");
                replacementStyleNode.SetAttributeValue("rel", "stylesheet");
                replacementStyleNode.SetAttributeValue("type", "text/css");
                replacementStyleNode.SetAttributeValue("href", LINQPadCssRelativePath);
                head.ReplaceChild(replacementStyleNode, styleNode);

                var scriptNode = htmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.ScriptNodePath);
                var replacementScriptNode = htmlDoc.CreateElement("script");
                replacementScriptNode.SetAttributeValue("src", LINQPadJsRelativePath);
                head.ReplaceChild(replacementScriptNode, scriptNode);
            }

            var linkHighlightJsCss = htmlDoc.CreateElement("link");
            linkHighlightJsCss.SetAttributeValue("rel", "stylesheet");
            linkHighlightJsCss.SetAttributeValue("type", "text/css");
            linkHighlightJsCss.SetAttributeValue("href", Globals.FileSystem.HighlightJsCssCdnUri);
            head.AppendChild(linkHighlightJsCss);

            var scriptHighlightJs = htmlDoc.CreateElement("script");
            scriptHighlightJs.SetAttributeValue("src", Globals.FileSystem.HighlightJsCdnUri);
            head.AppendChild(scriptHighlightJs);

            var scriptHighlightJsLoad = htmlDoc.CreateElement("script");
            scriptHighlightJsLoad.InnerHtml = Globals.FileSystem.HighlightJsLoadStatement;
            head.AppendChild(scriptHighlightJsLoad);

            var scriptMathJax = htmlDoc.CreateElement("script");
            scriptMathJax.SetAttributeValue("src", Globals.FileSystem.MathJaxCdnUri);
            head.AppendChild(scriptMathJax);

            var body = htmlDoc.DocumentNode.SelectSingleNode(Globals.DOM.BodyNodePath);

            if (scriptHtml.StartsWith(Globals.FileSystem.CodeSectionEnd))
            {
                scriptHtml = scriptHtml.Substring(Globals.FileSystem.CodeSectionEnd.Length);
            }

            if (scriptHtml.EndsWith(Globals.FileSystem.CodeSectionStart))
            {
                scriptHtml = scriptHtml.Substring(0, scriptHtml.Length - Globals.FileSystem.CodeSectionStart.Length);
            }
            else if (scriptHtml.EndsWith(Globals.FileSystem.CodeSectionStart + Environment.NewLine))
            {
                scriptHtml = scriptHtml.Substring(0, scriptHtml.Length - Globals.FileSystem.CodeSectionStart.Length - Environment.NewLine.Length);
            }
            else
            {
                scriptHtml += Globals.FileSystem.CodeSectionEnd;
            }

            var mainBody = htmlDoc.CreateTextNode(scriptHtml);
            body.AppendChild(mainBody);

            var dumpBody = htmlDoc.CreateTextNode(scriptInfo.ScriptOutput);
            body.AppendChild(dumpBody);

            return htmlDoc;
        }

        private class CommentRewriter : CSharpSyntaxRewriter
        {
            Markdown mark = new Markdown();

            public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
            {
                // By convention, LINQPadBlog only looks for markdown in multi-line trivia
                if (trivia.Kind() == SyntaxKind.MultiLineCommentTrivia)
                {
                    // Remove the leading "/*" from the comment
                    var multilineComment = trivia.ToString().Substring(2);
                    // Remove the trailing "*//r/n" from the comment
                    multilineComment = multilineComment.Substring(0, multilineComment.Length - 3);

                    // Assumes that all multi-line comments are embedded between a block of code 
                    // (should be true in general except at the start or end of the file, which is handled elsewhere)
                    // Use Markdown to transform any embedded markdown in the comments to html
                    return SyntaxFactory.Comment(Globals.FileSystem.CodeSectionEnd + mark.Transform(multilineComment) + Globals.FileSystem.CodeSectionStart);
                }
                else
                {
                    return base.VisitTrivia(trivia);
                }
            }
        }
    }
}
